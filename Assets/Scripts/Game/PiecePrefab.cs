using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PiecePrefab : MonoWithCachedTransform, IDragHandler, IBeginDragHandler, IEndDragHandler,
						   IPointerDownHandler, IPointerUpHandler
{
	public const float UPSCALE_OF_SELECTED_PIECE = 1.02f;
	public const float DOUBLE_TAP_BREAK_INTERVAL = 0.2f;
	public static bool IGNORE_BOUNDS_CHECK = true;
	public const float UPSCALE_BACKGROUND = 1f;
	public const float PIECE_MOVE_TO_BOARD_HORIZONTAL_THRESHOLD_WORLD_UNITS = 10f;

	public RawImage pieceRawImage;
	public RawImage pieceBackgroundImage;

	public Transform topRightCorner;
	public Transform bottomLeftCorner;

	public float Left { get { return bottomLeftCorner.position.x; } }
	public float Right { get { return topRightCorner.position.x; } }
	public float Top { get { return topRightCorner.position.y; } }
	public float Bottom { get { return bottomLeftCorner.position.y; } }

	[HideInInspector] public Transform pieceAnchor;
	[HideInInspector] public Transform anchorInPool;
	[HideInInspector] public Transform pieceOutlineContainer;

	private bool _isOnPlayingField;
	private float _lastTapDownTime;
	private float _lastTapUpTime;
	private bool _startedDoubleTap;

	public IntVector2 BoardPosition { get; private set; }

	public List<PiecePrefab> connectedPieces = new List<PiecePrefab>();
	public bool IsFreeStanding { get { return connectedPieces.Count == 1; } }
	public bool IsFullySurrounded { get; set; }

	private bool _forcedHighlight;
	private bool _backgroundAlreadyMovedToBackgroundDisplay;

	private IBoardService BoardService
	{
		get
		{
			return ServiceLocator.Get<IBoardService>();
		}
	}

	private Transform _backgroundTransform;
	public Transform BackgroundTransform
	{
		get
		{
			return _backgroundTransform ?? (_backgroundTransform = pieceBackgroundImage.transform);
		}
	}

	public void ConnectTo(PiecePrefab other)
	{
		if (IsNotYetConnectedTo(other))
		{
			connectedPieces.Add(other);

			IsFullySurrounded = IsFullySurrounded || CheckIfFullySurrounded();
		}
	}

	private bool CheckIfFullySurrounded()
	{
		var fullySurrounded = false;

		if (connectedPieces.Count > 4)
		{
			var top = BoardPosition + IntVector2.Up();
			var right = BoardPosition + IntVector2.Right();
			var left = BoardPosition + IntVector2.Left();
			var bottom = BoardPosition + IntVector2.Down();

			var surroundingPieceCount = 0;

			for (int i = 0; i < connectedPieces.Count && surroundingPieceCount < 4; ++i)
			{
				if (connectedPieces[i].BoardPosition == top ||
					connectedPieces[i].BoardPosition == right ||
					connectedPieces[i].BoardPosition == left ||
					connectedPieces[i].BoardPosition == bottom)
				{
					surroundingPieceCount++;
				}
			}

			fullySurrounded = surroundingPieceCount == 4;
		}

		return fullySurrounded;
	}

	private bool IsNotYetConnectedTo(PiecePrefab other)
	{
		foreach (var piece in connectedPieces)
		{
			if (other.BoardPosition == piece.BoardPosition)
			{
				return false;
			}
		}

		return true;
	}

	#region pointerHandlers

	public void OnBeginDrag(PointerEventData eventData)
	{
		if (!_isOnPlayingField)
		{
			PuzzlePiecePool.Instance.scrollRect.OnBeginDrag(eventData);
		}
	}

	public void OnDrag(PointerEventData eventData)
	{
		if (Input.touchCount > 1)
		{
			return;
		}

		var delta = new Vector3(eventData.delta.x, eventData.delta.y, 0f);

		if (_isOnPlayingField)
		{
			var newPosition = CachedTransform.position + delta;

			if (IGNORE_BOUNDS_CHECK || IsWithinBounds(newPosition))
			{
				foreach (var piece in connectedPieces)
				{
					piece.MoveBy(delta);
				}
			}

			BoardService.ShowPiecesInSnappingDistanceTo(connectedPieces, BoardPosition);
		}
		else
		{
			if (delta.x < -PIECE_MOVE_TO_BOARD_HORIZONTAL_THRESHOLD_WORLD_UNITS)
			{
				MoveToPlayingField();
			}
			else
			{
				PuzzlePiecePool.Instance.scrollRect.OnDrag(eventData);
			}
		}
	}

	public void OnEndDrag(PointerEventData eventData)
	{
		if (Input.touchCount > 1)
		{
			return;
		}

		if (_isOnPlayingField)
		{
			ScaleToNormalSize();
		}
		else
		{
			PuzzlePiecePool.Instance.scrollRect.OnEndDrag(eventData);
		}
	}

	public void OnPointerDown(PointerEventData eventData)
	{
		if (Input.touchCount > 1)
		{
			return;
		}

		if (_isOnPlayingField)
		{
			if (IsFreeStanding)
			{
				var now = Time.realtimeSinceStartup;
				var elapsedSinceLastTap = now - _lastTapUpTime;
				_startedDoubleTap = elapsedSinceLastTap <= DOUBLE_TAP_BREAK_INTERVAL;
				_lastTapDownTime = now;
			}

			foreach (var piece in connectedPieces)
			{
				piece.MoveToFront();
				piece.ScaleUp();
			}
		}
	}

	public void OnPointerUp(PointerEventData eventData)
	{
		if (_isOnPlayingField)
		{
			BoardService.ConnectPiecesWithinSnappingDistanceTo(this);
		}

		foreach (var piece in connectedPieces)
		{
			piece.ScaleToNormalSize();
		}

		if (IsFreeStanding && _isOnPlayingField)
		{
			_lastTapUpTime = Time.realtimeSinceStartup;
			if (_startedDoubleTap)
			{
				if (_lastTapUpTime - _lastTapDownTime <= DOUBLE_TAP_BREAK_INTERVAL)
				{
					MoveBackToPool();
				}
			}
		}
	}
	#endregion

	public void ScaleUp()
	{
		var upScaleFactor = BoardContext.DefaultPieceScaleFactor * UPSCALE_OF_SELECTED_PIECE;
		CachedTransform.localScale = new Vector3(upScaleFactor, upScaleFactor, 0f);
	}

	public void ScaleToNormalSize()
	{
		CachedTransform.localScale = new Vector3(BoardContext.DefaultPieceScaleFactor,
												 BoardContext.DefaultPieceScaleFactor, 0f);
	}

	private bool IsWithinBounds(Vector3 position)
	{
		return position.x > BoardContext.BottomLeftBounds.x && position.x < BoardContext.TopRightBounds.x &&
			   position.y > BoardContext.BottomLeftBounds.y && position.y < BoardContext.TopRightBounds.y;
	}

	public void MoveToFront()
	{
		pieceAnchor.SetAsLastSibling();
	}

	public void Setup(Texture2D paddedTexture, Vector2 uv, Vector2 pieceSize, int posx, int posy, bool isOnPlayingField)
	{
		var uvRect = new Rect(uv, pieceSize);
		pieceRawImage.texture = paddedTexture;
		pieceRawImage.uvRect = uvRect;

		pieceBackgroundImage.texture = paddedTexture;
		pieceBackgroundImage.uvRect = uvRect;

		BackgroundTransform.localScale = new Vector3(UPSCALE_BACKGROUND, UPSCALE_BACKGROUND, 0f);

		BoardPosition = new IntVector2(posx, posy);
		_isOnPlayingField = isOnPlayingField;
		if (_isOnPlayingField)
		{
			connectedPieces.Add(this);
		}
	}

	private void MoveBackToPool()
	{
		CachedTransform.SetParent(anchorInPool, false);
		pieceBackgroundImage.enabled = false;

		var rt = GetComponent<RectTransform>();
		rt.offsetMax = Vector2.zero;
		rt.offsetMin = Vector2.zero;

		connectedPieces.Clear();
		_isOnPlayingField = false;
		BoardService.MarkPieceInPool(this);
		anchorInPool.gameObject.SetActive(true);
	}

	public void MoveTo(Vector3 position)
	{
		CachedTransform.position = position;
		BackgroundTransform.position = position;
	}

	public void MoveBy(Vector3 delta)
	{
		var newPosition = CachedTransform.position + delta;
		MoveTo(newPosition);
	}

	public void HideHighlight()
	{
		if (!_forcedHighlight)
		{
			pieceBackgroundImage.enabled = false;
		}
	}

	public void ShowHighlightIfNotFullySurrounded()
	{
		if (!IsFullySurrounded)
		{
			pieceBackgroundImage.enabled = true;
		}
	}

	public void ForceShowHighlight(bool state)
	{
		_forcedHighlight = state;
		pieceBackgroundImage.enabled = state;
	}

	public void TryMoveBackgroundToBackgroundDisplay()
	{
		if (!_backgroundAlreadyMovedToBackgroundDisplay)
		{
			BackgroundTransform.localScale = new Vector3(UPSCALE_OF_SELECTED_PIECE, UPSCALE_OF_SELECTED_PIECE, 0f);
			BackgroundTransform.SetParent(pieceOutlineContainer, true);
			_backgroundAlreadyMovedToBackgroundDisplay = true;
		}
	}

	private Coroutine _moveToAnchorRoutine;

	// TODO: Disable movement while moving to anchored pos
	public void MoveToAnchoredPosition(float moveDurationSeconds)
	{
		if (_moveToAnchorRoutine == null)
		{
			_moveToAnchorRoutine = StartCoroutine(MoveToAnchoredPositionRoutine(moveDurationSeconds));
		}
	}

	private IEnumerator MoveToAnchoredPositionRoutine(float moveDurationSeconds)
	{
		var elapsedTime = 0f;
		var startPosition = CachedTransform.localPosition;

		while (elapsedTime < moveDurationSeconds)
		{
			var localPositionThisFrame = Vector3.Lerp(startPosition, Vector3.zero, elapsedTime / moveDurationSeconds);
			CachedTransform.localPosition = localPositionThisFrame;
			elapsedTime += Time.deltaTime;
			yield return null;
		}

		CachedTransform.localPosition = Vector3.zero;
		_moveToAnchorRoutine = null;
	}

	//TODO: Unify use of "highlight" vs "background"
	private void MoveToPlayingField()
	{
		var currentVerticalPosition = Mathf.Clamp(CachedTransform.position.y,
												  BoardContext.BottomLeftBounds.y,
												  BoardContext.TopRightBounds.y);

		CachedTransform.SetParent(pieceAnchor, true);
		CachedTransform.localScale = new Vector3(BoardContext.DefaultPieceScaleFactor, BoardContext.DefaultPieceScaleFactor, 0f);

		var rt = GetComponent<RectTransform>();
		rt.offsetMax = Vector2.zero;
		rt.offsetMin = Vector2.zero;

		TryMoveBackgroundToBackgroundDisplay();

		HideHighlight();
		MoveTo(new Vector3(BoardContext.TopRightBounds.x, currentVerticalPosition, 0f));

		anchorInPool.gameObject.SetActive(false);

		_isOnPlayingField = true;

		connectedPieces.Add(this);

		BoardService.MarkPieceOnPlayField(this);

		MoveToFront();
	}
}
