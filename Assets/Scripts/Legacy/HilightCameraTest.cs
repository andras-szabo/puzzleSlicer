using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HilightCameraTest : MonoBehaviour
{
	public Camera myCamera;
	public RawImage hilightCameraPreview;

	private void Start()
	{
		var renderTexture = new RenderTexture(Screen.width, Screen.height, 24);
		myCamera.targetTexture = renderTexture;

		hilightCameraPreview.texture = renderTexture;
	}
}
