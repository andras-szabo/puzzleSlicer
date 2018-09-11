using UnityEngine;

public struct Dimension
{
	public int currentRow;
	public int currentColumn;
	public int rows;
	public int columns;
}

public struct PaddingInfo
{
	public Vector2 padding;
	public Vector2 paddedPieceSize;
	public IntVector2 paddedPixelCount;
}

public static class TextureUtility
{
	public const string LEGACY_SAVED_TEXTURE_FILE = "_activeTexture";

	public static void CleanupLegacyTextureData()
	{
		var legacyDataPath = Paths.GetFullDataPath(LEGACY_SAVED_TEXTURE_FILE);
		if (System.IO.File.Exists(legacyDataPath))
		{
			System.IO.File.Delete(legacyDataPath);
			Debug.Log("Cleaned up legacy texture data.");
		}
	}

	public static bool TryLoadSavedTexture(Vector2 originalSize, int rows, int columns,
										   SlicedTextureInfo slicedTextureInfo, string originalTexturePath,
										   out Texture2D slicedTexture)
	{
		var slicedTextureSizeFactor = 1f + (ImgSlicer.PADDING_RATIO * 2f);

		var slicedTextureSize = originalSize * slicedTextureSizeFactor;
		var slicedTexSizeX = Mathf.CeilToInt(slicedTextureSize.x);
		var slicedTexSizeY = Mathf.CeilToInt(slicedTextureSize.y);

		slicedTexture = new Texture2D(slicedTexSizeX, slicedTexSizeY);

		var pathToSavedTexture = Paths.GetFullPathToSavedTexture();

		if (System.IO.File.Exists(pathToSavedTexture) && slicedTextureInfo != null && slicedTextureInfo.originalPath == originalTexturePath)
		{
			var success = true;
			try
			{
				var rawTextureData = System.IO.File.ReadAllBytes(pathToSavedTexture);
				ImageConversion.LoadImage(slicedTexture, rawTextureData);
			}
			catch (System.Exception e)
			{
				Debug.LogWarning(e.Message);
				success = false;
			}

			if (success)
			{
				return true;
			}
		}

		slicedTexture.SetPixels(new Color[slicedTexSizeX * slicedTexSizeY], 0);
		return false;
	}

	public static void CopyMaskedPieceDataIntoPiecePixelsArray(Dimension dimensions,
				 Texture2D paddedTexture,
				 PaddingInfo paddingInfo,
				 IntVector2 sourceOffset,
				 Color[] piecePixels,
				 Color[] maskPixels,
				 Color borderColor)
	{
		var sourceColor = new Color();
		var emptyColor = new Color();

		var outlineColor = new Color(0.1f, 0f, 0f, 0f);

		int offsetX = Mathf.Max(0, sourceOffset.x);
		int offsetY = Mathf.Max(0, sourceOffset.y);

		if (dimensions.currentColumn == dimensions.columns - 1) { offsetX = paddedTexture.width - paddingInfo.paddedPixelCount.x; }
		if (dimensions.currentRow == dimensions.rows - 1) { offsetY = paddedTexture.height - paddingInfo.paddedPixelCount.y; }

		var pieceRectangle = paddedTexture.GetPixels(offsetX, offsetY,
													 paddingInfo.paddedPixelCount.x,
													 paddingInfo.paddedPixelCount.y);

		for (int x = 0; x < paddingInfo.paddedPixelCount.x; ++x)
		{
			var maskDataXOffset = (int)(x / paddingInfo.paddedPieceSize.x * MaskCreator.width);
			for (int y = 0; y < paddingInfo.paddedPixelCount.y; ++y)
			{
				var maskData = maskPixels[(int)((y / paddingInfo.paddedPieceSize.y) * MaskCreator.height) * MaskCreator.width
										+ maskDataXOffset];

				sourceColor = emptyColor;

				var currentPixelVisible = maskData.a > 0.5f;
				var isBorder = maskData.r > 0.3f;
				var isHighlight = maskData.b > 0.3f;

				if (currentPixelVisible)
				{
					if (isBorder)
					{
						sourceColor = borderColor;
					}
					else
					{
						if ((dimensions.currentRow == 0 && y < paddingInfo.padding.y) ||
							(dimensions.currentRow == dimensions.rows - 1 && y > (paddingInfo.paddedPieceSize.y - paddingInfo.padding.y)) ||
							(dimensions.currentColumn == 0 && x < paddingInfo.padding.x) ||
							(dimensions.currentColumn == dimensions.columns - 1 && x > (paddingInfo.paddedPieceSize.x - paddingInfo.padding.x)))
						{
							//sourceColor = emptyColor;
						}
						else
						{
							if (!isHighlight)
							{
								var actualOffsetX = x;
								var actualOffsetY = y;

								if (dimensions.currentColumn == 0) { actualOffsetX -= (int) paddingInfo.padding.x; }
								if (dimensions.currentRow == 0) { actualOffsetY -= (int)paddingInfo.padding.y; }

								if (dimensions.currentColumn == dimensions.columns - 1) { actualOffsetX += (int)paddingInfo.padding.x; }
								if (dimensions.currentRow == dimensions.rows - 1) { actualOffsetY += (int)paddingInfo.padding.y; }

								sourceColor = pieceRectangle[actualOffsetX  + (actualOffsetY * paddingInfo.paddedPixelCount.x)];
							}
							else
							{
								sourceColor = outlineColor;
							}
						}
					}
				}

				piecePixels[x + (y * paddingInfo.paddedPixelCount.x)] = sourceColor;
			}
		}
	}

	public static void ExportTexture(Texture2D texture, int rows, int columns, Vector2 originalSize, string originalTexturePath)
	{
		var pngData = ImageConversion.EncodeToPNG(texture);
		var success = false;

		try
		{
			var path = Paths.GetFullPathToSavedTexture();
			System.IO.File.WriteAllBytes(path, pngData);
			success = true;

			Debug.LogFormat("Saved texture: {0}, size: {1} bytes ({2} mb)", 
							path, pngData.Length, (float) pngData.Length / (1024f * 1024f));
		}
		catch (System.IO.IOException exc)
		{
			Debug.LogWarning(exc.Message);
		}

		if (success)
		{
			SaveTextureInfo(rows, columns, originalSize, originalTexturePath);
		}
	}

	public static void SaveTextureInfo(int rows, int columns, Vector2 originalSize, string originalTexturePath)
	{
		var info = new SlicedTextureInfo
		{
			rows = rows,
			columns = columns,
			originalPath = originalTexturePath,
			originalSizeX = (int)originalSize.x,
			originalSizeY = (int)originalSize.y
		};

		var asJson = JsonUtility.ToJson(info, true);
		var path = Paths.GetFullPathToSavedTextureInfo();

		try
		{
			System.IO.File.WriteAllText(path, asJson);
		}
		catch (System.Exception e)
		{
			Debug.LogWarning(e.Message);
		}

		Debug.LogWarning("Saved info to: " + path);
	}

	public static bool TryGetSavedTextureInfo(out SlicedTextureInfo savedTextureInfo)
	{
		var pathToSavedFileInfo = Paths.GetFullPathToSavedTextureInfo();

		savedTextureInfo = null;

		if (System.IO.File.Exists(pathToSavedFileInfo))
		{
			string asJson = "";

			try
			{
				asJson = System.IO.File.ReadAllText(pathToSavedFileInfo);
				if (!string.IsNullOrEmpty(asJson))
				{
					savedTextureInfo = JsonUtility.FromJson<SlicedTextureInfo>(asJson);
				}
			}
			catch (System.Exception e)
			{
				Debug.LogWarning(e.Message);
			}
		}

		return savedTextureInfo != null;
	}
}
