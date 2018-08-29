using System.Collections.Generic;
using UnityEngine;

public class PuzzleService : MonoSingleton<PuzzleService>
{
	public static float pieceWidth;
	public static float pieceHeight;

	public static Vector3 bottomLeftBounds;
	public static Vector3 topRightBounds;

	public static float pieceScaleFactor;

	private List<PiecePrefab> _piecesOnBoard = new List<PiecePrefab>();
	private List<PiecePrefab> _piecesNotBeingDraggedRightNow = new List<PiecePrefab>();

	private IntVector2 _pieceBeingDragged;
	private List<PiecePrefab> _piecesWithinSnappingDistance = new List<PiecePrefab>();

	private GameState _gameState = new GameState();

	public Transform pieceOutlineDisplay;
	public Transform puzzleContainer;

	public void Reset()
	{
		_piecesOnBoard.Clear();
		_piecesNotBeingDraggedRightNow.Clear();
		_piecesWithinSnappingDistance.Clear();
		_pieceBeingDragged = new IntVector2();
		_gameState.Clear();
	}

	private void OnApplicationPause(bool pause)
	{
		if (pause)
		{
			TrySaveGameState();
		}
	}

	public void TrySaveGameState()
	{
		if (_gameState != null)
		{
			_gameState.Save();
		}
	}

	public GameState TryLoadSavedGameState()
	{
		if (_gameState.TryLoad())
		{
			return _gameState;
		}

		return null;
	}

	public void ConnectLoadedPieces()
	{
		foreach (var root in _gameState.Roots)
		{
			var pieceToKeepSteady = FindPiece(root.boardPosition);
			foreach (var connectedPosition in root.connectedBoardPositions)
			{
				if (connectedPosition != root.boardPosition)
				{
					var connectedPiece = FindPiece(connectedPosition);
					SnapToPieceAt(pieceToKeepSteady, connectedPiece);
				}
			}
		}

		foreach (var root in _gameState.Roots)
		{
			for (int i = 0; i < root.connectedBoardPositions.Count; ++i)
			{
				var a = FindPiece(root.connectedBoardPositions[i]);
				for (int j = i + 1; j < root.connectedBoardPositions.Count; ++j)
				{
					var b = FindPiece(root.connectedBoardPositions[j]);
					ConnectPieces(a, b);
				}
			}
		}
	}

	private PiecePrefab FindPiece(IntVector2 position)
	{
		foreach (var piece in _piecesOnBoard)
		{
			if (piece.BoardPosition == position)
			{
				return piece;
			}
		}

		return null;
	}

	public void MarkPieceOnPlayingField(PiecePrefab piece)
	{
		_piecesOnBoard.Add(piece);
		_gameState.AddToPlayField(piece.BoardPosition, piece.transform.position);
	}

	public void MarkPieceInPool(PiecePrefab piece)
	{
		_piecesOnBoard.Remove(piece);
		_gameState.RemoveFromPlayField(piece.BoardPosition);
	}

	public void ShowPiecesInSnappingDistanceTo(List<PiecePrefab> piecesBeingDragged, IntVector2 pieceBeingMovedPosition)
	{
		if (pieceBeingMovedPosition != _pieceBeingDragged)
		{
			// When looking for snappable pieces, we compare the position of one set of
			// connected pieces (that are being moved) to that of every other piece. But
			// we don't need to recalculate all the time what "every other piece" means;
			// so we cache it and only recalculate when the user starts a new drag.

			PrepareComparisonFilter(piecesBeingDragged);
			_pieceBeingDragged = pieceBeingMovedPosition;
		}

		foreach (var piece in _piecesWithinSnappingDistance)
		{
			piece.ScaleToNormalSize();
			piece.HideHighlight();
		}

		_piecesWithinSnappingDistance.Clear();

		var piecesDirectlyTouchingMovedPiece = GetPiecesDirectlyTouchingMovedPiece(piecesBeingDragged);

		foreach (var piece in piecesDirectlyTouchingMovedPiece)
		{
			_piecesWithinSnappingDistance.Add(piece);
		}

		HighlightPiecesInSnappingDistance(piecesBeingDragged, piecesDirectlyTouchingMovedPiece);
	}

	private void HighlightPiecesInSnappingDistance(List<PiecePrefab> piecesBeingDragged, List<PiecePrefab> piecesToHighlight)
	{
		if (piecesToHighlight.Count < 1) { return; }

		// piece background should go above all the pieces that are being moved,
		// or being highlighted. => e.g. if 7 pieces are moved or highlighted, then
		// background should go to [length - 7 - 1]
		// highlighted pieces should go to an index after that,
		// and then moved pieces accordingly.

		var totalSiblingCount = puzzleContainer.childCount;
		var siblingIndex = totalSiblingCount - piecesBeingDragged.Count - piecesToHighlight.Count;

		pieceOutlineDisplay.SetSiblingIndex(siblingIndex++);

		foreach (var piece in piecesToHighlight)
		{
			piece.ScaleUp();
			piece.pieceAnchor.SetSiblingIndex(siblingIndex++);
			piece.ShowHighlightIfNotFullySurrounded();
		}

		foreach (var piece in piecesBeingDragged)
		{
			piece.pieceAnchor.SetSiblingIndex(siblingIndex++);
		}
	}

	public void BringFreeStandingPiecesToTheFront()
	{
		foreach (var piece in _piecesOnBoard)
		{
			if (piece.IsFreeStanding)
			{
				piece.MoveToFront();
			}
		}
	}

	public Vector3 GetWorldPositionForPiece(IntVector2 boardPosition)
	{
		var piece = FindPiece(boardPosition);

		if (piece != null)
		{
			return piece.transform.position;
		}

		return Vector3.zero;
	}

	public void ConnectPiecesWithinSnappingDistanceTo(PiecePrefab piece)
	{
		if (_piecesWithinSnappingDistance.Count > 0)
		{
			foreach (var pieceToSnap in _piecesWithinSnappingDistance)
			{
				pieceToSnap.HideHighlight();
				var connectedPieces = new List<PiecePrefab>(piece.connectedPieces);
				foreach (var connectedPiece in connectedPieces)
				{
					SnapToPieceAt(connectedPiece, pieceToSnap);
					ConnectPieces(connectedPiece, pieceToSnap);
					connectedPiece.HideHighlight();
				}
			}
		}

		_gameState.UpdatePositionAndSaveConnections(piece);

		_piecesWithinSnappingDistance.Clear();
		_pieceBeingDragged = new IntVector2(-1, -1);
	}

	private void ConnectPieces(PiecePrefab a, PiecePrefab b)
	{
		a.ConnectTo(b);
		b.ConnectTo(a);
	}

	private void SnapToPieceAt(PiecePrefab pieceToKeepSteady, PiecePrefab pieceToMove)
	{
		var positionDifference = pieceToMove.BoardPosition - pieceToKeepSteady.BoardPosition;

		var delta = new Vector3(positionDifference.x * pieceWidth, positionDifference.y * pieceHeight);
		pieceToMove.MoveTo(pieceToKeepSteady.transform.position + delta);
	}

	//TODO: optimize
	private List<PiecePrefab> GetPiecesDirectlyTouchingMovedPiece(List<PiecePrefab> piecesBeingDragged)
	{
		var piecesTouching = new List<PiecePrefab>();

		foreach (var piece in piecesBeingDragged)
		{
			foreach (var pieceOnBoard in _piecesNotBeingDraggedRightNow)
			{
				if (AdjacencyHelper.IsWithinSnappingDistance(piece, pieceOnBoard))
				{
					foreach (var connectedPiece in pieceOnBoard.connectedPieces)
					{
						if (!Contains(piecesTouching, connectedPiece))
						{
							piecesTouching.Add(connectedPiece);
						}
					}
				}
			}
		}

		return piecesTouching;
	}

	public static bool Contains(List<PiecePrefab> list, PiecePrefab item)
	{
		foreach (var piece in list)
		{
			if (piece.BoardPosition == item.BoardPosition)
			{
				return true;
			}
		}

		return false;
	}

	private void PrepareComparisonFilter(List<PiecePrefab> piecesBeingDragged)
	{
		_piecesNotBeingDraggedRightNow.Clear();

		foreach (var pieceOnBoard in _piecesOnBoard)
		{
			var isBeingDragged = false;

			foreach (var piece in piecesBeingDragged)
			{
				if (pieceOnBoard.BoardPosition == piece.BoardPosition)
				{
					isBeingDragged = true;
					break;
				}
			}

			if (!isBeingDragged)
			{
				_piecesNotBeingDraggedRightNow.Add(pieceOnBoard);
			}
		}
	}
}
