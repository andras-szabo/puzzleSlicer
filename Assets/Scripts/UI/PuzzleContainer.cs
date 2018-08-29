using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
public class PuzzleContainer : MonoBehaviour
{
	public RectTransform pieceAnchorPrototype;
	public Transform origin;
	public Transform topRightBounds;
	public Transform bottomLeftBounds;
	private Dictionary<IntVector2, Transform> _pieceAnchors = new Dictionary<IntVector2, Transform>();

	public RawImage helperBackgroundImg;

	private void Cleanup()
	{
		foreach (var anchor in _pieceAnchors.Values)
		{
			Destroy(anchor.gameObject);
		}

		_pieceAnchors.Clear();
	}

	public void ToggleBackground()
	{
		var currentState = helperBackgroundImg.gameObject.activeSelf;
		helperBackgroundImg.gameObject.SetActive(!currentState);
	}

	public void Setup(SlicingInfo sliceInfo, Texture2D imageToSlice)
	{
		Cleanup();

		var imgWidth = imageToSlice.width;
		var imgHeight = imageToSlice.height;

		helperBackgroundImg.texture = imageToSlice;
		
		var relativeWidth = 1f / (float)sliceInfo.columns;
		var relativeHeight = 1f / (float)sliceInfo.rows;

		var myRect = GetComponent<RectTransform>().rect;

		ScaleToFitSourceImage(imgWidth, imgHeight, myRect);

		myRect = GetComponent<RectTransform>().rect;

		var actualWidth = myRect.width / sliceInfo.columns;
		var actualHeight = myRect.height / sliceInfo.rows;

		for (int i = 0; i < sliceInfo.columns; ++i)
		{
			for (int j = 0; j < sliceInfo.rows; ++j)
			{
				var pieceAnchor = UnityEngine.Object.Instantiate<RectTransform>(pieceAnchorPrototype, parent: this.transform);

				var anchorPosX = (relativeWidth / 2f) + (i * relativeWidth);
				var anchorPosY = (relativeHeight / 2f) + (j * relativeHeight);
				var anchor = new Vector2(anchorPosX, anchorPosY);

				pieceAnchor.anchorMin = anchor;
				pieceAnchor.anchorMax = anchor;

				pieceAnchor.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, actualWidth);
				pieceAnchor.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, actualHeight);

				_pieceAnchors.Add(new IntVector2(j, i), pieceAnchor);
			}
		}

		var anchorDifference = _pieceAnchors[new IntVector2(1, 1)].transform.position - _pieceAnchors[new IntVector2(0, 0)].transform.position;

		PuzzleService.pieceWidth = anchorDifference.x;
		PuzzleService.pieceHeight = anchorDifference.y;
	}

	private void ScaleToFitSourceImage(int imgWidth, int imgHeight, Rect myrect)
	{
		var imgAspectRatio = (float)imgWidth / (float)imgHeight;
		var currentAspectRatio = myrect.width / myrect.height;

		if (currentAspectRatio >= imgAspectRatio)
		{
			var widthScaleFactor = (myrect.height * (float)imgWidth) / (myrect.width * (float)imgHeight);
			transform.localScale = new Vector3(widthScaleFactor, 1f, 1f);
		}

		if (currentAspectRatio < imgAspectRatio)
		{
			var heightScaleFactor = (myrect.width * (float)imgHeight) / ((float)imgWidth * myrect.height);
			transform.localScale = new Vector3(1f, heightScaleFactor, 0f);
		}
	}

	public Transform GetPieceAnchor(int x, int y)
	{
		return _pieceAnchors[new IntVector2(x, y)];
	}
}

[System.Serializable]
public struct IntVector2
{
	public static IntVector2 Up()
	{
		return new IntVector2(0, 1);
	}

	public static IntVector2 Right()
	{
		return new IntVector2(1, 0);
	}

	public static IntVector2 Left()
	{
		return new IntVector2(-1, 0);
	}

	public static IntVector2 Down()
	{
		return new IntVector2(0, -1);
	}

	public static bool operator == (IntVector2 a, IntVector2 b)
	{
		return a.x == b.x && a.y == b.y;
	}

	public static bool operator != (IntVector2 a, IntVector2 b)
	{
		return !(a == b);
	}

	public static IntVector2 operator - (IntVector2 a, IntVector2 b)
	{
		return new IntVector2(a.x - b.x, a.y - b.y);
	}

	public static IntVector2 operator +(IntVector2 a, IntVector2 b)
	{
		return new IntVector2(a.x + b.x, a.y + b.y);
	}

	public override bool Equals(object obj)
	{
		if (obj == null) { return false; }
		var other = (IntVector2)obj;
		return other == this;
	}

	public override int GetHashCode()
	{
		return base.GetHashCode();
	}

	public override string ToString()
	{
		return string.Format("({0}; {1})", x, y);
	}

	public int x, y;

	public IntVector2(int x, int y)
	{
		this.x = x;
		this.y = y;
	}
}
