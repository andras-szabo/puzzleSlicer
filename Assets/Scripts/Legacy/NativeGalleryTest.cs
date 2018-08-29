using UnityEngine;
using UnityEngine.UI;

public class NativeGalleryTest : MonoBehaviour
{
	public RawImage display;

	private void Start()
	{
		Invoke("DoTheThing", 2f);
	}

	private void DoTheThing()
	{
		var permission = NativeGallery.GetImageFromGallery(OnImagePicked, "Please pick an img");
		Debug.Log(permission);
	}

	private void OnImagePicked(string imgPath)
	{
		if (!string.IsNullOrEmpty(imgPath))
		{
			var tex = NativeGallery.LoadImageAtPath(imgPath, maxSize: -1, 
													markTextureNonReadable: false,
													generateMipmaps: false);
			if (tex != null)
			{
				display.texture = tex;
			}
		}
	}
}
