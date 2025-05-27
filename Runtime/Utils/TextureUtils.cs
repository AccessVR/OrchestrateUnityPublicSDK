using System;
using UnityEngine;
using Cysharp.Threading.Tasks;
using UnityEngine.Networking;

namespace AccessVR.OrchestrateVR.SDK
{
	public class TextureUtils
	{
		public static async UniTask<Texture2D> LoadTexture2D(string path)
		{
			string uri = "file://" + path;
            Debug.Log("Loading Texture from " + uri);
            UnityWebRequest request = UnityWebRequestTexture.GetTexture(uri, true);
            await request.SendWebRequest();
            if (request.isDone)
            {
                return DownloadHandlerTexture.GetContent(request);
            }
            else
            {
	            // TODO: bubble up error
	            Debug.LogError($"Failed to load texture: {uri}");
	            return new Texture2D(1, 1);
            }
		}

		public static async UniTask<Texture2D> LoadTexture2D(FileData file)
		{
			if (!file.IsCached)
			{
				throw new Exception($"File is not cached: ${file.Name}");
			}
			return await LoadTexture2D(file.CachePath);
		}
	}
}


