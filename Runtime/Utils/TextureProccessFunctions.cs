using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

public static class TextureProccessFunctions
{
	//downlaod file async
	//take file and turn it into a texture
	//send texture back

	//async public Texture2D DownloadTexture(string urlPath)
	//{
	//	//web request download
	//	UnityWebRequest www = UnityWebRequestTexture.GetTexture(urlPath);
	//	UnityWebRequestAsyncOperation operation = www.SendWebRequest();
	//}

	#region Enumerator Method, Must be changed
	private class TextureWebFunctionsMonoBehaviour : MonoBehaviour { }
	private static TextureWebFunctionsMonoBehaviour webMonoBehaviour;

	private static void Init()
	{
		if(webMonoBehaviour == null)
		{
			GameObject go = new GameObject("TextureFunctions_WebRequests");
			webMonoBehaviour = go.AddComponent<TextureWebFunctionsMonoBehaviour>();
		}
	}

	public static void DownloadImage(string urlPath, Action<string> onError, Action<Texture2D> onSuccess)
	{
		Init();
		webMonoBehaviour.StartCoroutine(DownloadImageEnumerator(urlPath, onError, onSuccess));
	}

	private static IEnumerator DownloadImageEnumerator(string urlPath, Action<string> onError, Action<Texture2D> onSuccess)
	{
		using (UnityWebRequest www = UnityWebRequestTexture.GetTexture(urlPath))
		{
			yield return www.SendWebRequest();

			if (www.result != UnityWebRequest.Result.Success)
			{
				onError(www.error);
			}
			else
			{
				onSuccess(((DownloadHandlerTexture)www.downloadHandler).texture);
			}
		}
	}
	#endregion

	public static Texture2D LoadTextureLocalFile(string filePath)
	{
		if (File.Exists(filePath))
		{
			Texture2D tex = new Texture2D(1, 1);
			tex.LoadImage(File.ReadAllBytes(filePath));
			return tex;
		}
		else
		{
			Debug.LogError("Failed to load texture from disk");
			Texture2D tex = new Texture2D(1, 1);
			return tex;
		}
	}

	public static Sprite ConvertTextureToSprite(Texture2D tex)
	{
		return Sprite.Create(tex, new Rect (0,0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
	}
}
