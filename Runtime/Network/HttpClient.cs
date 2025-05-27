using UnityEngine;
using System;
using System.Net;
using System.Net.Http;
using Cookie = System.Net.Cookie;
using BaseHttpClient = System.Net.Http.HttpClient;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using Newtonsoft.Json.Linq;

namespace AccessVR.OrchestrateVR.SDK
{
    public class HttpClient : BaseHttpClient
    {
	    
        private HttpClient(HttpClientHandler handler) : base(handler)
        {
			//
        }

        public static HttpClient Create([NotNull] string baseUrl, [CanBeNull] string authToken = null)
        {
            CookieContainer cookies = new CookieContainer();
			HttpClientHandler handler = new HttpClientHandler();
			handler.CookieContainer = cookies;

			Uri uri = new Uri(baseUrl);

			Cookie deviceCookie = new Cookie("device-id", SystemInfo.deviceUniqueIdentifier);
			deviceCookie.Domain = uri.Host;
			cookies.Add(deviceCookie);

			var client = new HttpClient(handler);

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

        public async UniTask<bool> IsLoggedIn()
        {
	        try
	        {
		        HttpResponseMessage response = await GetAsync(Url("/api/rest/player-manifest"));
		        return response.StatusCode == HttpStatusCode.OK;
	        }
	        catch (HttpRequestException e)
	        {
		        Debug.Log("Failed to download Player Manifest: " + e.Message);
		        return false;
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
			
			if (!String.IsNullOrEmpty(lookup.EmbedKey))
			{
				path += "/" + lookup.EmbedKey;
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

        public async UniTask<SubmissionData> Submit(LessonData lesson)
        {
	        // var content = new StringContent("{\"LessonSubmission\":{\"AssignmentId\":\"" + Orchestrate.Instance.currentAssignment?.guid + "\", \"LessonId\":\"" + lessonData.Id + "\",\"LearnerId\":\"" + User.UserId + "\",\"LearnerName\":\"" + User.DisplayName + "\", \"Score\":\"" + lessonData.Score + "\" , \"CompletedOn\":\"" + DateTime.Now.ToString("U") + "\"}}", System.Text.Encoding.UTF8, "application/json");
			
			// TODO: build content from lesson object
			StringContent content = new StringContent("{}");

			//Debug.Log("Record scores " + content);

			HttpResponseMessage response = await PostAsync(Url("/api/rest/lesson-submission/create"), content);
			string responseBody = await HttpUtils.AssertSuccessfulResponse(response);

			// TODO: build new SubmissionData record to capture changes in response
			return new SubmissionData();
        }

    }
}