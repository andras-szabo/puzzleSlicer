using UnityEngine;

public static class MaskCreator
{
	public static int width = 280;
	public static int height = 280;
	public static int padding = 80;

	public static Color32[] maskData = new Color32[width * padding];


	public static void CreateMask(Color[] maskLeft, Color[] maskTop,
									   Color[] maskRight, Color[] maskBottom,
									   Color[] fullMask)
	{
		ClearWorkingArray(fullMask);

		if (maskTop != null) CopyToTop(maskTop, fullMask, padding);
		if (maskRight != null) CopyToRight(maskRight, fullMask, padding);
		if (maskBottom != null) CopyToBottom(maskBottom, fullMask, padding);
		if (maskLeft != null) CopyToLeft(maskLeft, fullMask, padding);

		ClearEdges(fullMask);
	}

	private static void ClearWorkingArray(Color[] pixelsToCombine)
	{
		var green = new Color(0f, 1f, 0f, 1f);

		for (int i = 0; i < pixelsToCombine.Length; ++i)
		{
			pixelsToCombine[i] = green;
		}
	}

	private static void ClearEdges(Color[] pixelsToCombine)
	{
		var edgeToleranceInPixels = 2;

		var empty = new Color();

		for (int i = 0; i < edgeToleranceInPixels; ++i)
		{
			for (int j = 0; j < width; ++j)
			{
				pixelsToCombine[j + (i * width)] = empty;					// bottom rows
				pixelsToCombine[j + ((height - 1 - i) * width)] = empty;	// top rows
			}

			for (int j = 0; j < height; ++j)
			{
				pixelsToCombine[i + (j * width)] = empty;                   // columns on left edge
				pixelsToCombine[(width - 1 - i) + (j * width)] = empty;		// columns on the right edge
			}
		}
	}

	private static void CopyToTop(Color[] maskData, Color[] pixelsToCombine, int padding)
	{
		for (int row = 0; row < padding; ++row)
		{
			for (int col = 0; col < width; ++col)
			{
				var source = row * width + col;

				var dest = pixelsToCombine.Length - 1 - source;
				pixelsToCombine[dest] = maskData[source];
			}
		}
	}

	private static void CopyToBottom(Color[] maskData, Color[] pixelsToCombine, int padding)
	{
		for (int row = 0; row < padding; ++row)
		{
			for (int col = 0; col < width; ++col)
			{
				var source = row * width + col;
				pixelsToCombine[source] = maskData[source];
			}
		}
	}

	private static void CopyToLeft(Color[] maskData, Color[] pixelsToCombine, int padding)
	{
		for (int row = 0; row < padding; ++row)
		{
			for (int col = 0; col < width; ++col)
			{
				var source = row * width + col;
				var dstRow = col;
				var dstCol = row;
				var dest = dstRow * width + dstCol;
				pixelsToCombine[dest] = maskData[source];
			}
		}
	}

	private static void CopyToRight(Color[] maskData, Color[] pixelsToCombine, int padding)
	{
		for (int row = 0; row < padding; ++row)
		{
			for (int col = 0; col < width; ++col)
			{
				var source = row * width + col;

				var dstRow = col;
				var dstCol = row;

				var dest = pixelsToCombine.Length - 1 - (dstRow * width + dstCol);
				pixelsToCombine[dest] = maskData[source];
			}
		}
	}
}
