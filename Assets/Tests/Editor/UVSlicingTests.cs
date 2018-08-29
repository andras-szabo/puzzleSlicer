using UnityEngine;
using NUnit.Framework;

public class UVSlicingTests
{
	[Test]
	public void CalculatePaddingTest()
	{
		Vector2 imgSize = new Vector2(800f, 600f);

		var padding = ImgSlicer.GetPaddingSize(imgSize, rows: 5, columns: 5);
		Assert.IsTrue(FEqual(padding.x, (imgSize.x / 5f) * ImgSlicer.PADDING_RATIO));
		Assert.IsTrue(FEqual(padding.y, (imgSize.y / 5f) * ImgSlicer.PADDING_RATIO));
	}

	[Test]
	public void SliceCountTests01()
	{
		Vector2 imgSize = new Vector2(800f, 600f);

		// Slice the longer side to 8 pieces => it's expected
		// that the shorter one will be cut to 6 pieces, so as
		// to have pieces as square as possible.

		const int pieceCountForLongerSide = 8;
		var imgSliceInfo = ImgSlicer.GetSliceInfo(imgSize, pieceCountForLongerSide);
		Assert.IsTrue(imgSliceInfo.rows == 6 && imgSliceInfo.columns == 8, imgSliceInfo.ToString());

		imgSize = new Vector2(600f, 800f);
		imgSliceInfo = ImgSlicer.GetSliceInfo(imgSize, pieceCountForLongerSide);
		Assert.IsTrue(imgSliceInfo.rows == 8 && imgSliceInfo.columns == 6, imgSliceInfo.ToString());
	}

	[Test]
	public void SliceCountTests02()
	{
		Vector2 imgSize = new Vector2(1024f, 768f);

		// 1024 x 768, longer side cut to 6 pieces

		const int pieceCountForLongerSide = 6;
		var imgSliceInfo = ImgSlicer.GetSliceInfo(imgSize, pieceCountForLongerSide);
		Assert.IsTrue(imgSliceInfo.rows == 4 && imgSliceInfo.columns == 6, imgSliceInfo.ToString());

		// 900 x 1600, longer side cut to 6 pieces

		imgSize = new Vector2(900f, 1600f);
		imgSliceInfo = ImgSlicer.GetSliceInfo(imgSize, pieceCountForLongerSide);
		Assert.IsTrue(imgSliceInfo.rows == 6 && imgSliceInfo.columns == 3, imgSliceInfo.ToString());

		// 900 x 1600, longer side to 7 pieces

		imgSliceInfo = ImgSlicer.GetSliceInfo(imgSize, pieceCountForLongerSide: 7);
		Assert.IsTrue(imgSliceInfo.rows == 7 && imgSliceInfo.columns == 4, imgSliceInfo.ToString());
	}

	[Test]
	public void SliceCountTests03()
	{
		var imgSize = new Vector2(1024f, 768f);
		var imgSliceInfo = ImgSlicer.GetSliceInfo(imgSize, Difficulty.Easy);

		// longer side to 4 pieces => shorter to 3

		Assert.IsTrue(imgSliceInfo.rows == 3 && imgSliceInfo.columns == 4, imgSliceInfo.ToString());

		// longer side avg(4, 1024 / 50)/2 = 12, shorter side to 768 / (1024 / 12) = 9

		imgSliceInfo = ImgSlicer.GetSliceInfo(imgSize, Difficulty.Medium);
		Assert.IsTrue(imgSliceInfo.rows == 9 && imgSliceInfo.columns == 12, imgSliceInfo.ToString());

		// longer side to 20, shorter side to 768 / (1024 / 20) = 15

		imgSliceInfo = ImgSlicer.GetSliceInfo(imgSize, Difficulty.Hard);
		Assert.IsTrue(imgSliceInfo.rows == 15 && imgSliceInfo.columns == 20, imgSliceInfo.ToString());
	}

	[Test]
	public void CalculatePaddedImageSizeTest()
	{
		Vector2 imgSize = new Vector2(200f, 200f);
		var paddedImgSize = ImgSlicer.GetPaddedImageSize(imgSize, rows: 4, columns: 4);
		Assert.IsTrue(FEqual(paddedImgSize.x, (imgSize.x / 4f * ImgSlicer.PADDING_RATIO) * 2f + imgSize.x));
		Assert.IsTrue(FEqual(paddedImgSize.y, (imgSize.y / 4f * ImgSlicer.PADDING_RATIO) * 2f + imgSize.y));
	}

	[Test]
	public void CalculateUVsTestFor33()
	{
		Vector2 imgSize = new Vector2(200f, 200f);
		var rows = 4;
		var cols = 4;

		var pieceSize = 50f + 2f * 10f;
		var paddedSize = ImgSlicer.GetPaddedImageSize(imgSize, rows, cols);

		Assert.IsTrue(FEqual(paddedSize.x, 220f) && FEqual(paddedSize.y, 220f));

		var expectedUVs = new Vector2[]
		{
			new Vector2(1f - (pieceSize / paddedSize.x), 1f - (pieceSize / paddedSize.y)),
			new Vector2(1f, 1f - (pieceSize / paddedSize.y)),
			new Vector2(1f - (pieceSize / paddedSize.x), 1f),
			new Vector2(1f, 1f)
		};

		var uvs = ImgSlicer.GetUVsForPiece(rows, cols, imgSize, 3, 3);

		for (int i = 0; i < uvs.Length; ++i)
		{
			Assert.IsTrue(VEqual(uvs[i], expectedUVs[i]), string.Format("Mismatch: {0} vs {1} // {2}", uvs[i], expectedUVs[i], i));
		}
	}

	[Test]
	public void CalculateUVsTestFor00()
	{
		Vector2 imgSize = new Vector2(200f, 200f);
		var rows = 4;
		var cols = 4;

		var paddedSize = ImgSlicer.GetPaddedImageSize(imgSize, rows, cols);
		// padding: 10, pieceWidth: 50, pieceHeight: 50

		var pieceSize = 50f + 2f * 10f;

		var expectedUVs = new Vector2[]
		{
			Vector2.zero,
			new Vector2(pieceSize / paddedSize.x, 0f),
			new Vector2(0f, pieceSize / paddedSize.y),
			new Vector2(pieceSize / paddedSize.x, pieceSize / paddedSize.y)
		};

		var uvs = ImgSlicer.GetUVsForPiece(rows, cols, imgSize, 0, 0);

		for (int i = 0; i < uvs.Length; ++i)
		{
			Assert.IsTrue(VEqual(uvs[i], expectedUVs[i]), string.Format("Mismatch: {0} vs {1}", uvs[i], expectedUVs[i]));
		}
	}

	private static bool VEqual(Vector2 a, Vector2 b)
	{
		return FEqual(a.x, b.x) && FEqual(a.y, b.y);
	}

	private static bool FEqual(float a, float b)
	{
		return Mathf.Approximately(a, b);
	}
}
