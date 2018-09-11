using UnityEngine;

public class BoardContext : MonoSingleton<BoardContext>
{
	public static float PieceWidthInWorldUnits { get { return Instance._pieceSizeInWorldUnits.x; } }
	public static float PieceHeightInWorldUnits { get { return Instance._pieceSizeInWorldUnits.y; } }

	public static Vector3 BottomLeftBounds { get { return Instance.bottomLeftBounds; } }
	public static Vector3 TopRightBounds { get { return Instance.topRightBounds; } }

	public static float DefaultPieceScaleFactor { get { return Instance.defaultPieceScaleFactor; } }

	private Vector2 _pieceSizeInWorldUnits;

	private Vector3 bottomLeftBounds;
	private Vector3 topRightBounds;

	private float defaultPieceScaleFactor;

	public void Setup(float defaultScaleFactor, Vector3 topRightBounds, Vector3 bottomLeftBounds)
	{
		this.defaultPieceScaleFactor = defaultScaleFactor;
		this.topRightBounds = topRightBounds;
		this.bottomLeftBounds = bottomLeftBounds;
	}

	public void SetPieceDimensions(Vector2 pieceSizeInWorldUnits)
	{
		_pieceSizeInWorldUnits = pieceSizeInWorldUnits;
	}

	public void AdjustPieceDimensions(float incrementFactor)
	{
		_pieceSizeInWorldUnits *= incrementFactor;
	}
}
