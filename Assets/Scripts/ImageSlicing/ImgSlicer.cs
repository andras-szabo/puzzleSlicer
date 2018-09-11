using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ImgSlicer
{
	public static float PADDING_RATIO = 0.2f;
	public static float DOUBLE_PADDING_RATIO = PADDING_RATIO * 2f;
	public static int MAX_PIECE_COUNT_PER_ROW = 12;

	public static Connection Inverse(this Connection connection)
	{
		switch (connection)
		{
			case Connection.Hook: return Connection.Eye;
			case Connection.Eye: return Connection.Hook;
		}

		return Connection.None;
	}

	public static SlicingInfo GetSliceInfo(Vector2 imgSize, Difficulty difficulty)
	{
		var minPieceCount = 4;
		var maxPieceCount = Mathf.Min(MAX_PIECE_COUNT_PER_ROW,
									 (int)Mathf.Max(Mathf.Max(imgSize.x, imgSize.y) / 50f, minPieceCount));

		var pieceCountForLongerSide = minPieceCount;
		switch (difficulty)
		{
			case Difficulty.Medium: pieceCountForLongerSide = (minPieceCount + maxPieceCount) / 2; break;
			case Difficulty.Hard: pieceCountForLongerSide = maxPieceCount; break;
		}

		return GetSliceInfo(imgSize, pieceCountForLongerSide);
	}

	public static SlicingInfo GetSliceInfo(Vector2 imgSize, int pieceCountForLongerSide)
	{
		var longerSide = Mathf.Max(imgSize.x, imgSize.y);
		var shorterSide = Mathf.Min(imgSize.x, imgSize.y);

		var pieceSize = longerSide / pieceCountForLongerSide;
		var pieceCountForShorterSide = Mathf.RoundToInt(shorterSide / pieceSize);

		return new SlicingInfo(imgSize, pieceCountForLongerSide, pieceCountForShorterSide);
	}

	public static Vector2 GetPaddingSize(Vector2 imageSize, int rows, int columns)
	{
		var pieceWidth = imageSize.x / (float)columns;
		var pieceHeight = imageSize.y / (float)rows;

		return new Vector2(pieceWidth * PADDING_RATIO, pieceHeight * PADDING_RATIO);
	}

	public static Vector2 GetPaddedImageSize(Vector2 imageSize, int rows, int columns)
	{
		var padding = GetPaddingSize(imageSize, rows, columns);
		return new Vector2(imageSize.x + 2f * padding.x, imageSize.y + 2f * padding.y);
	}

	public static Vector2 GetPaddedPieceSize(int rows, int columns, Vector2 imgSizeWithoutPadding)
	{
		var padding = GetPaddingSize(imgSizeWithoutPadding, rows, columns);
		var pieceWidth = imgSizeWithoutPadding.x / (float)columns;
		var pieceHeight = imgSizeWithoutPadding.y / (float)rows;

		var paddedPieceWidth = pieceWidth + 2 * padding.x;
		var paddedPieceHeight = pieceHeight + 2 * padding.y;

		return new Vector2(paddedPieceWidth, paddedPieceHeight);
	}

	public static Vector2 GetBottomLeftPixelForPaddedPiece(int rows, int columns,
														   Vector2 imgSizeWithoutPadding,
														   int column, int row)
	{
		var bottomLeft = new Vector2();

		var padding = GetPaddingSize(imgSizeWithoutPadding, rows, columns);

		var pieceWidth = imgSizeWithoutPadding.x / (float)columns;
		var pieceHeight = imgSizeWithoutPadding.y / (float)rows;

		var paddedPieceWidth = pieceWidth + 2 * padding.x;
		var paddedPieceHeight = pieceHeight + 2 * padding.y;

		bottomLeft.x = column * paddedPieceWidth;
		bottomLeft.y = row * paddedPieceHeight;

		return bottomLeft;
	}

	public static Vector2[] GetUVsForPiece(int rows, int columns,
										   Vector2 imgSizeWithoutPadding,
										   int posx, int posy)
	{
		var padding = GetPaddingSize(imgSizeWithoutPadding, rows, columns);
		var paddedImgSize = GetPaddedImageSize(imgSizeWithoutPadding, rows, columns);

		var pieceWidth = imgSizeWithoutPadding.x / (float)columns;
		var pieceHeight = imgSizeWithoutPadding.y / (float)rows;

		var paddedPieceWidth = pieceWidth + 2 * padding.x;
		var paddedPieceHeight = pieceHeight + 2 * padding.y;

		var left = (posx * pieceWidth) / paddedImgSize.x;
		var right = (posx * pieceWidth + paddedPieceWidth) / paddedImgSize.x;

		var bottom = (posy * pieceHeight) / paddedImgSize.y;
		var top = (posy * pieceHeight + paddedPieceHeight) / paddedImgSize.y;

		return new Vector2[]
		{
			new Vector2(left, bottom),
			new Vector2(right, bottom),
			new Vector2(left, top),
			new Vector2(right, top)
		};
	}

	// This is how slicing works:
	// take the original image, and copy it in chunks - each equal to a size of a piece, including padding around the sides
	// to the new "sliced" texture. But when copying, also do a UV lookup into the array that represents the mask; and only
	// copy those pixels that are not blocked (and also take the masks's borders into consideration). So essentially bake
	// the masked pieces onto a larger texture. Then we can use that one and only texture, with different UVs, to draw every
	// single piece.

	public static IEnumerator CreateAndSaveSlicedTextureRoutine(SlicingInfo slicingInfo, Texture2D originalTexture,
																Texture2D slicedTexture,
																string originalTexturePath,
																MaskContainer maskContainer,
																Dictionary<IntVector2, PieceInfo> pieceInfos,
																System.Action<float> onProgress)
	{
		//TODO maybe we could rationalize these
		var rows = slicingInfo.rows;
		var columns = slicingInfo.columns;
		var originalSize = new Vector2(originalTexture.width, originalTexture.height);

		var paddedPieceSize = GetPaddedPieceSize(rows, columns, originalSize);
		var pieceSize = new Vector2((float)originalSize.x / (float)columns,
									(float)originalSize.y / (float)rows);
		var padding = (paddedPieceSize - pieceSize) / 2f;
		var borderColor = Color.black;

		var paddedPixelCountX = Mathf.FloorToInt(paddedPieceSize.x);
		var paddedPixelCountY = Mathf.FloorToInt(paddedPieceSize.y);

		var piecePixels = new Color[paddedPixelCountX * paddedPixelCountY];
		var maskPixels = new Color[MaskCreator.width * MaskCreator.height];

		onProgress(0f);

		var thingsToLoadCount = rows * columns + 1; // "+1" for the extra step in the end, assigning textures etc.
													// this is just cosmetics- showing the loading indicator
		var thingsLoadedSoFar = 0f;

		for (int column = 0; column < columns; ++column)
		{
			for (int row = 0; row < rows; ++row)
			{
				CreateMask(maskContainer, pieceInfos, row, column, maskPixels);

				var sourceOffsetX = Mathf.RoundToInt(pieceSize.x * column - padding.x);
				var sourceOffsetY = Mathf.RoundToInt(pieceSize.y * row - padding.y);

				var destOffsetX = Mathf.RoundToInt(paddedPieceSize.x * column);
				var destOffsetY = Mathf.RoundToInt(paddedPieceSize.y * row);

				var dimensions = new Dimension
				{
					currentRow = row,
					currentColumn = column,
					rows = rows,
					columns = columns
				};

				var paddingInfo = new PaddingInfo
				{
					padding = padding,
					paddedPieceSize = paddedPieceSize,
					paddedPixelCount = new IntVector2(paddedPixelCountX, paddedPixelCountY)
				};

				TextureUtility.CopyMaskedPieceDataIntoPiecePixelsArray(dimensions, originalTexture, paddingInfo,
																		new IntVector2(sourceOffsetX, sourceOffsetY),
																		piecePixels, maskPixels, borderColor);

				slicedTexture.SetPixels(destOffsetX, destOffsetY, paddedPixelCountX, paddedPixelCountY, piecePixels, 0);

				thingsLoadedSoFar += 1f;
				onProgress(thingsLoadedSoFar / thingsToLoadCount);
				yield return null;
			}
		}

		TextureUtility.ExportTexture(slicedTexture, rows, columns, originalSize, originalTexturePath);
	}

	private static void CreateMask(MaskContainer maskContainer, Dictionary<IntVector2, PieceInfo> pieceInfos,
						    int row, int column, Color[] maskPixels)
	{
		var pieceInfo = pieceInfos[new IntVector2(column, row)];

		var left = maskContainer.GetTextureAsArray(pieceInfo.edgeLeft);
		var top = maskContainer.GetTextureAsArray(pieceInfo.edgeTop);
		var right = maskContainer.GetTextureAsArray(pieceInfo.edgeRight);
		var bottom = maskContainer.GetTextureAsArray(pieceInfo.edgeBottom);

		MaskCreator.CreateMask(left, top, right, bottom, maskPixels);
	}

	public static Dictionary<IntVector2, PieceInfo> SetupConnections(SlicingInfo sliceInfo)
	{
		var rows = sliceInfo.rows;
		var columns = sliceInfo.columns;
		var pieceInfos = new Dictionary<IntVector2, PieceInfo>();

		for (int col = 0; col < columns; ++col)
		{
			for (int row = 0; row < rows; ++row)
			{
				pieceInfos.Add(new IntVector2(col, row), new PieceInfo(col, row));
			}
		}

		for (int col = 0; col < columns; ++col)
		{
			for (int row = 0; row < rows; ++row)
			{
				if (col + 1 < columns)
				{
					SetupConnectionToRight(col, row, pieceInfos);
				}

				if (row + 1 < rows)
				{
					SetupConnectionToTop(col, row, pieceInfos);
				}
			}
		}

		return pieceInfos;
	}

	public static void SetupConnectionToRight(int col, int row, Dictionary<IntVector2, PieceInfo> pieceInfos)
	{
		var myConnection = Random.Range(0f, 1f) < 0.5f ? Connection.Hook : Connection.Eye;
		var otherConnection = myConnection.Inverse();

		pieceInfos[new IntVector2(col, row)].edgeRight = myConnection;
		pieceInfos[new IntVector2(col + 1, row)].edgeLeft = otherConnection;
	}

	public static void SetupConnectionToTop(int col, int row, Dictionary<IntVector2, PieceInfo> pieceInfos)
	{
		var myConnection = Random.Range(0f, 1f) < 0.5f ? Connection.Hook : Connection.Eye;
		var otherConnection = myConnection.Inverse();

		pieceInfos[new IntVector2(col, row)].edgeTop = myConnection;
		pieceInfos[new IntVector2(col, row + 1)].edgeBottom = otherConnection;
	}
}
