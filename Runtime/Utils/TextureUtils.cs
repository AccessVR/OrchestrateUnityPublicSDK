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
			using UnityWebRequest request = UnityWebRequest.Get(uri);
			await request.SendWebRequest();

			if (request.result != UnityWebRequest.Result.Success)
			{
				Debug.LogError($"Failed to load texture: {uri} ({request.error})");
				return new Texture2D(1, 1);
			}

			// Build with a mip chain and trilinear/anisotropic filtering. These images are
			// shown on world-space canvases, where any minification (distance, oblique angle,
			// head motion) makes an un-mipmapped texture shimmer/crawl in VR. Creating the
			// texture with mipChain:true lets LoadImage generate the mip levels on decode.
			var texture = new Texture2D(2, 2, TextureFormat.RGBA32, mipChain: true);
			texture.LoadImage(request.downloadHandler.data, markNonReadable: true);
			texture.filterMode = FilterMode.Trilinear;
			texture.anisoLevel = 4;
			return texture;
		}
	}
}
