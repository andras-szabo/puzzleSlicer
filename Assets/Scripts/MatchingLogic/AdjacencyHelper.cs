using UnityEngine;

public static class AdjacencyHelper
{
	public const float SNAP_TOLERANCE_Y = 20f;
	public const float SNAP_TOLERANCE_X = 20f;

	public static bool IsWithinSnappingDistance(PiecePrefab a, PiecePrefab b)
	{
		var adjacency = IsAdjacent(a, b);

		switch (adjacency)
		{
			case Adjacency.ToRight:
				{
					var verticalMatch = IsApproximately(a.transform.position.y, b.transform.position.y, SNAP_TOLERANCE_Y);
					return verticalMatch && OverlapsToRight(a, b);
				}

			case Adjacency.ToLeft:
				{
					var verticalMatch = IsApproximately(a.transform.position.y, b.transform.position.y, SNAP_TOLERANCE_Y);
					return verticalMatch && OverlapsToLeft(a, b);
				}

			case Adjacency.ToTop:
				{
					var horizontalMatch = IsApproximately(a.transform.position.x, b.transform.position.x, SNAP_TOLERANCE_X);
					return horizontalMatch && OverlapsToTop(a, b);
				}

			case Adjacency.ToBottom:
				{
					var horizontalMatch = IsApproximately(a.transform.position.x, b.transform.position.x, SNAP_TOLERANCE_X);
					return horizontalMatch && OverlapsToBottom(a, b);
				}
		}

		return false;
	}

	public static bool OverlapsToBottom(PiecePrefab a, PiecePrefab b)
	{
		return a.Bottom < b.Top && a.Bottom > (b.Top - ((b.Top - b.Bottom) / 5f));
	}

	public static bool OverlapsToTop(PiecePrefab a, PiecePrefab b)
	{
		return a.Top > b.Bottom && a.Top < ((b.Top - b.Bottom) / 5f) + b.Bottom;
	}

	public static bool OverlapsToRight(PiecePrefab a, PiecePrefab b)
	{
		return a.Right > b.Left && a.Right < ((b.Right - b.Left) / 5f) + b.Left;
	}

	public static bool OverlapsToLeft(PiecePrefab a, PiecePrefab b)
	{
		return a.Left < b.Right && a.Left > (b.Right - ((b.Right - b.Left) / 5f));
	}

	public static bool IsApproximately(float a, float b, float tolerance)
	{
		return b >= (a - tolerance) && b <= (a + tolerance);
	}

	public static Adjacency IsAdjacent(PiecePrefab a, PiecePrefab b)
	{
		var boardDistance = a.BoardPosition - b.BoardPosition;

		if (boardDistance.x == 0 && (Mathf.Abs(boardDistance.y) == 1))
		{
			return boardDistance.y > 0 ? Adjacency.ToBottom : Adjacency.ToTop;
		}

		if (boardDistance.y == 0 && (Mathf.Abs(boardDistance.x) == 1))
		{
			return boardDistance.x > 0 ? Adjacency.ToLeft : Adjacency.ToRight;
		}

		return Adjacency.None;
	}
}

public enum Adjacency
{
	None,
	ToRight,
	ToLeft,
	ToTop,
	ToBottom
}
