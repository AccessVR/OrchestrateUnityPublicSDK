using UnityEngine;
using System;
using System.Net;
using System.Net.Http;
using System.ComponentModel;
using System.IO;
using Cookie = System.Net.Cookie;
using BaseHttpClient = System.Net.Http.HttpClient;

namespace AccessVR.OrchestrateVR.SDK
{
    public class HttpClient : BaseHttpClient
    {
        private HttpClient(HttpClientHandler handler) : base(handler)
        {

        }

        public static HttpClient Create()
        {
            CookieContainer cookies = new CookieContainer();
			HttpClientHandler handler = new HttpClientHandler();
			handler.CookieContainer = cookies;

			Uri uri = new Uri(OrchestrateEnvironment.GetUrl("/"));

			Cookie deviceCookie = new Cookie("device-id", SystemInfo.deviceUniqueIdentifier); //UID for the device this program is running on (aka the headset or unity editor)
			deviceCookie.Domain = uri.Host;
			cookies.Add(deviceCookie);

			var client = new HttpClient(handler);

			if (OrchestrateEnvironment.GetAuthToken() != null) {
				client.DefaultRequestHeaders.Add("Authorization", "Bearer " + OrchestrateEnvironment.GetAuthToken()); //tells thing to use token to log in
			}
			client.DefaultRequestHeaders.Add("Accept", "application/json");

			return client;
        }

    }
}