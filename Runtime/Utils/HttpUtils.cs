using Newtonsoft.Json.Linq;
using System.Net.Http;
using Cysharp.Threading.Tasks;
using System;
using System.Collections.Specialized;
using System.Web;

namespace AccessVR.OrchestrateVR.SDK
{
    public class HttpUtils
    {

        /// <summary>
		/// Given an HttpResponseMessage, if the response is not a success state, 
		/// try to parse an error message from the response body.
		/// </summary>
		/// <param name="response"></param>
		/// <returns></returns>
		/// <exception cref="HttpRequestException"></exception>
		/// <exception cref="Exception"></exception>
		public static async UniTask<string> AssertSuccessfulResponse(HttpResponseMessage response)
		{
			string responseBody = await response.Content.ReadAsStringAsync();
			if (!response.IsSuccessStatusCode)
			{
				JObject error = null;
				try
				{
					error = JObject.Parse(responseBody);
				} catch
                {
					// in this circumstance, EnsureSuccessStatusCode() will throw an exception
					response.EnsureSuccessStatusCode();
					return "";
                }
				if (error != null)
				{
					throw new HttpRequestException(error["exception"] + ": " + error["message"] + " in " + error["file"] + "@" + error["line"]);
				} else
                {
					throw new Exception("Invalid state");
                }

			} else
            {
				return responseBody;
            }
		}

		public static NameValueCollection ParseQueryString(string query)
		{
			return HttpUtility.ParseQueryString(query);
		}
    }
}