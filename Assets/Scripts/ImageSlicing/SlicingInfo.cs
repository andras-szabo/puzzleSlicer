using UnityEngine;

public struct SlicingInfo
{
	public readonly int imgWidthInPixels;
	public readonly int imgHeightInPixels;

	public readonly int rows;
	public readonly int columns;

	public override string ToString()
	{
		return string.Format("[SlicingInfo] ImgSize: {0}, {1}. Rows: {2}, cols: {3}",
							imgWidthInPixels, imgHeightInPixels, rows, columns);
	}

	public SlicingInfo(Vector2 imgSize, int pieceCountForLongerSide, int pieceCountForShorterSide)
	{
		imgWidthInPixels = (int)imgSize.x;
		imgHeightInPixels = (int)imgSize.y;

		var tallerThanItIsWide = imgHeightInPixels > imgWidthInPixels;

		rows = tallerThanItIsWide ? pieceCountForLongerSide : pieceCountForShorterSide;
		columns = tallerThanItIsWide ? pieceCountForShorterSide : pieceCountForLongerSide;
	}

	public SlicingInfo(SlicedTextureInfo textureInfo)
	{
		imgWidthInPixels = textureInfo.originalSizeX;
		imgHeightInPixels = textureInfo.originalSizeY;

		rows = textureInfo.rows;
		columns = textureInfo.columns;
	}
}
