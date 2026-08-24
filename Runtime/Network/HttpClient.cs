using UnityEngine;
using UnityEngine.Networking;
using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Cookie = System.Net.Cookie;
using BaseHttpClient = System.Net.Http.HttpClient;
using System.Collections.Generic;
using System.Text;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AccessVR.OrchestrateVR.SDK
{
    /// <summary>
    /// A lesson submission the server refused with a non-retryable status
    /// (4xx other than 429). Carries the status so the durable-queue flusher
    /// can hold a possibly-stale-auth row (403) or drop a permanently
    /// unacceptable one, instead of retrying every failure forever.
    /// </summary>
    public class SubmissionRejectedException : Exception
    {
        public int StatusCode { get; }

        public SubmissionRejectedException(int statusCode, string body)
            : base($"Submission rejected with HTTP {statusCode}: {body}")
        {
            StatusCode = statusCode;
        }
    }

    internal class LoggingHandler : DelegatingHandler
    {
        public LoggingHandler(HttpMessageHandler innerHandler) : base(innerHandler) { }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Debug.Log($"[HTTP] {request.Method} {request.RequestUri}");
            return await base.SendAsync(request, cancellationToken);
        }
    }

    public class HttpClient : BaseHttpClient
    {
	    
        private HttpClient(HttpMessageHandler handler) : base(handler)
        {
			//
        }

        public static HttpClient Create([NotNull] string baseUrl, [CanBeNull] string authToken = null)
        {
            CookieContainer cookies = new CookieContainer();
			HttpClientHandler handler = new HttpClientHandler();
			handler.CookieContainer = cookies;
			handler.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
			// API endpoints all return JSON directly; a redirect (e.g., 302 → /login)
			// means the request was rejected (typically auth-related). Following
			// the redirect would yield a 200 HTML page that misleads the caller into
			// treating the request as successful.
			handler.AllowAutoRedirect = false;

			Uri uri = new Uri(baseUrl);

			Cookie deviceCookie = new Cookie("device-id", SystemInfo.deviceUniqueIdentifier);
			deviceCookie.Domain = uri.Host;
			cookies.Add(deviceCookie);

			var loggingHandler = new LoggingHandler(handler);
			var client = new HttpClient(loggingHandler);
			client.Timeout = TimeSpan.FromSeconds(30);

			if (authToken != null) {
				client.DefaultRequestHeaders.Add("Authorization", "Bearer " + authToken);
			}
			client.DefaultRequestHeaders.Add("Accept", "application/json");

			return client;
        }

        private string Url(string path)
        {
	        return Orchestrate.GetUrl(path);
        }

        // Routed through UnityWebRequest (NSURLSession on iOS) with an explicit
        // poll-and-deadline loop. System.Net.Http.HttpClient hangs indefinitely on
        // iPadOS 26 when player-manifest returns a 302, even with an explicit
        // Timeout and AllowAutoRedirect = false. UnityWebRequest's own `timeout`
        // field also doesn't fire reliably here, so we enforce a deadline via
        // Time.realtimeSinceStartup and abort the request ourselves.
        public async UniTask<bool> IsLoggedIn()
        {
	        var url = Url("/api/rest/player-manifest");
	        Debug.Log($"[HTTP] GET {url}");
	        using (var req = UnityWebRequest.Get(url))
	        {
		        req.redirectLimit = 0;
		        req.SetRequestHeader("Accept", "application/json");
		        var token = Orchestrate.GetAuthToken();
		        if (!string.IsNullOrEmpty(token))
		        {
			        req.SetRequestHeader("Authorization", "Bearer " + token);
		        }
		        var op = req.SendWebRequest();
		        var deadline = Time.realtimeSinceStartup + 15f;
		        while (!op.isDone)
		        {
			        if (Time.realtimeSinceStartup > deadline)
			        {
				        Debug.Log("[HTTP] IsLoggedIn deadline exceeded; aborting");
				        req.Abort();
				        return false;
			        }
			        await UniTask.Yield();
		        }
		        var ok = req.result == UnityWebRequest.Result.Success && req.responseCode == 200;
		        Debug.Log($"[HTTP] IsLoggedIn result={req.result} code={req.responseCode} ok={ok}");
		        return ok;
	        }
        }

        public async UniTask<string> GetUserCode(string deviceUniqueIdentifier)
        {
	        StringContent content = new StringContent("{\"DeviceCode\":\"" + deviceUniqueIdentifier + "\"}", System.Text.Encoding.UTF8, "application/json");
			HttpResponseMessage response = await PostAsync(Url("/api/rest/auth/device-code"), content);
	        string responseBody = await HttpUtils.AssertSuccessfulResponse(response);
	        JObject rootObject = JObject.Parse(responseBody);
	        return rootObject["userCode"].ToString();
        }

        public async UniTask<string> GetAuthToken(string userCode)
        {
	        StringContent content = new StringContent("{\"DeviceCode\":\"" + SystemInfo.deviceUniqueIdentifier + "\", \"UserCode\":\"" + userCode + "\"}", System.Text.Encoding.UTF8, "application/json");
			HttpResponseMessage response = await PostAsync(Url("/api/rest/auth/request-api-key"), content);
			
			if (response.StatusCode == HttpStatusCode.OK)
			{
				string responseBody = await response.Content.ReadAsStringAsync();
				return JObject.Parse(responseBody)["apiKey"]?.ToString();
			}

			Debug.Log("Checked for API Key; device not paired yet: " + response.StatusCode);
			return null;
        }

        public async UniTask<string> GetSkyboxPath()
        {
			HttpResponseMessage response = await GetAsync(Url("/api/rest/player-manifest/skybox"));
			string responseBody = await HttpUtils.AssertSuccessfulResponse(response);
			return JObject.Parse(responseBody)["path"]?.ToString();
        }

        public async UniTask<List<AssignmentData>> GetUserAssignments(string userId)
        {
	        HttpResponseMessage response =
		        await GetAsync(Url($"/api/rest/assignment/learner/{userId}"));
	        string responseBody = await HttpUtils.AssertSuccessfulResponse(response);
	        return JArray.Parse(responseBody).ToObject<List<AssignmentData>>();
        }

        public async UniTask<UserData> GetUser([CanBeNull] string userId = null)
        {
	        string path = "/api/json/reply/Authenticate";
	        if (!String.IsNullOrEmpty(userId))
	        {
		        path = $"/api/users/{userId}";
	        }
	        HttpResponseMessage response = await GetAsync(Url(path));
	        string responseBody = await HttpUtils.AssertSuccessfulResponse(response);
			JObject rootObject = JObject.Parse(responseBody);
			return rootObject.ToObject<UserData>();
        }

        public async UniTask<LessonData> GetLesson(LessonDataLookup lookup)
        {
	        string path = "/api/rest/published-lesson/" + lookup.Id;
			
			if (!String.IsNullOrEmpty(lookup.UniqueKey))
			{
				path += "/" + lookup.UniqueKey;
			}
			
			if (!String.IsNullOrEmpty(lookup.Preview))
			{
				path += "?preview=" + UnityWebRequest.EscapeURL(lookup.Preview);
			}
			
			HttpResponseMessage response = await GetAsync(Url(path));
			string responseBody = await HttpUtils.AssertSuccessfulResponse(response);
			JObject data = JObject.Parse(responseBody)["result"].ToObject<JObject>();
			string guid = data["guid"]?.ToString();
	        LessonData lessonData = data["content"].ToObject<LessonData>();
			lessonData.Guid = guid;
			// Version identity sits beside `content` on the Lesson resource,
			// like guid; stamped here so it round-trips through the cache.
			lessonData.PublishedLessonId = data["publishedLessonId"]?.Value<int?>();
			lessonData.PublishedVersionNumber = data["publishedVersionNumber"]?.Value<int?>();
			lessonData.IsPreview = !String.IsNullOrEmpty(lookup.Preview);
			// The share key comes from the launch context, not the payload.
			// Stamped so it round-trips through the lesson cache and rides
			// the submission — unlisted content authorizes play by this key.
			lessonData.UniqueKey = lookup.UniqueKey;
			return lessonData;
        }

        /// <summary>
        /// Delays between submission attempts. The server is idempotent on
        /// abxrRunId, which is what makes retrying safe; retry only on
        /// no-response, 429, or 5xx — any other 4xx is a real rejection and
        /// fails fast. A Retry-After header, when present, replaces the
        /// scheduled delay.
        /// </summary>
        private static readonly TimeSpan[] SubmitRetryDelays =
        {
	        TimeSpan.FromSeconds(1),
	        TimeSpan.FromSeconds(4),
	        TimeSpan.FromSeconds(10),
        };

        public async UniTask<SubmissionData> Submit(SubmissionData submission)
        {
	        string payload = JsonConvert.SerializeObject(submission);
	        Debug.Log(payload);
	        string url = Url("/api/rest/lesson-submission/create");

	        for (int attempt = 0; ; attempt++)
	        {
		        HttpResponseMessage response = null;
		        try
		        {
			        // Content must be rebuilt per attempt: HttpContent cannot
			        // be reused across sends.
			        StringContent encodedPayload = new StringContent(payload, Encoding.UTF8, "application/json");
			        response = await PostAsync(url, encodedPayload);
		        }
		        catch (Exception exception) when (exception is HttpRequestException or TaskCanceledException)
		        {
			        if (attempt >= SubmitRetryDelays.Length)
			        {
				        throw;
			        }
			        Debug.LogWarning($"[Submit] no response (attempt {attempt + 1}): {exception.Message}");
			        await UniTask.Delay(SubmitRetryDelays[attempt]);
			        continue;
		        }

		        if (ShouldRetrySubmit(response) && attempt < SubmitRetryDelays.Length)
		        {
			        TimeSpan delay = RetryAfterOrDefault(response, SubmitRetryDelays[attempt]);
			        Debug.LogWarning($"[Submit] HTTP {(int) response.StatusCode} (attempt {attempt + 1}); retrying in {delay.TotalSeconds:0}s");
			        await UniTask.Delay(delay);
			        continue;
		        }

		        if (!response.IsSuccessStatusCode && !ShouldRetrySubmit(response))
		        {
			        // The server refused this submission outright (auth,
			        // missing lesson, bad payload) — waiting will not fix it.
			        // A typed throw lets the durable-queue flusher decide
			        // whether to hold or drop the row, instead of treating
			        // every failure as transient.
			        string body = await response.Content.ReadAsStringAsync();
			        throw new SubmissionRejectedException((int) response.StatusCode, body);
		        }

		        string responseBody = await HttpUtils.AssertSuccessfulResponse(response);
		        return JsonConvert.DeserializeObject<SubmissionData>(responseBody);
	        }
        }

        private static bool ShouldRetrySubmit(HttpResponseMessage response)
        {
	        return (int) response.StatusCode == 429 || (int) response.StatusCode >= 500;
        }

        private static TimeSpan RetryAfterOrDefault(HttpResponseMessage response, TimeSpan fallback)
        {
	        var retryAfter = response.Headers.RetryAfter;
	        if (retryAfter?.Delta != null)
	        {
		        return retryAfter.Delta.Value;
	        }
	        if (retryAfter?.Date != null)
	        {
		        TimeSpan untilDate = retryAfter.Date.Value - DateTimeOffset.UtcNow;
		        return untilDate > TimeSpan.Zero ? untilDate : fallback;
	        }
	        return fallback;
        }

    }
}