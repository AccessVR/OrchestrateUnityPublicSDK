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
			
			if (lookup.Preview)
			{
				path += "?preview=1";
			}
			
			HttpResponseMessage response = await GetAsync(Url(path));
			string responseBody = await HttpUtils.AssertSuccessfulResponse(response);
			JObject data = JObject.Parse(responseBody)["result"].ToObject<JObject>();
			string guid = data["guid"]?.ToString();
	        LessonData lessonData = data["content"].ToObject<LessonData>();
			lessonData.Guid = guid;
			return lessonData;
        }

        public async UniTask<SubmissionData> Submit(SubmissionData submission)
        {
	        string payload = JsonConvert.SerializeObject(submission);
	        StringContent encodedPayload = new StringContent(payload, Encoding.UTF8, "application/json");
	        Debug.Log(payload);
	        string url = Url("/api/rest/lesson-submission/create");
	        HttpResponseMessage response = await PostAsync(url, encodedPayload);
			string responseBody = await HttpUtils.AssertSuccessfulResponse(response);
			return JsonConvert.DeserializeObject<SubmissionData>(responseBody);
        }

    }
}