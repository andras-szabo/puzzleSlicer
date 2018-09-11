using System;
using System.Collections.Generic;
using UnityEngine;

public interface IBoardService : IService
{
	event Action OnPlayerHasWon;

	void Setup(int totalPieceCount, Transform outlines, Transform puzzleContainer);
	void SnapAndConnectLoadedPieces();
	void MarkPieceOnPlayField(PiecePrefab piece);
	void MarkPieceInPool(PiecePrefab piece);
	void ShowPiecesInSnappingDistanceTo(List<PiecePrefab> piecesBeingDragged, IntVector2 pieceBeingMovedPosition);
	void ConnectPiecesWithinSnappingDistanceTo(PiecePrefab piece);
	void BringFreeStandingPiecesToTheFront();

	Vector3 GetWorldPositionForPiece(IntVector2 boardPosition);
}

public class BoardService : IBoardService
{
	public event Action OnPlayerHasWon;

	private List<PiecePrefab> _piecesOnBoard = new List<PiecePrefab>();
	private List<PiecePrefab> _piecesNotBeingDraggedRightNow = new List<PiecePrefab>();
	private IntVector2 _pieceBeingDragged;
	private List<PiecePrefab> _piecesWithinSnappingDistance = new List<PiecePrefab>();

	private int _totalPieceCount = -1;

	private Transform _pieceOutlineDisplay;
	private Transform _puzzleContainer;

	private IGameStateService _gameStateService;

	public void Init()
	{
		_piecesOnBoard.Clear();
		_piecesNotBeingDraggedRightNow.Clear();
		_piecesWithinSnappingDistance.Clear();
		_pieceBeingDragged = new IntVector2();
		_totalPieceCount = -1;

		_gameStateService = ServiceLocator.Get<IGameStateService>();
	}

	public void Setup(int totalPieceCount, Transform pieceOutlineDisplay, Transform puzzleContainer)
	{
		_totalPieceCount = totalPieceCount;
		_pieceOutlineDisplay = pieceOutlineDisplay;
		_puzzleContainer = puzzleContainer;
		_gameStateService.Setup(_totalPieceCount);
	}

	public void SnapAndConnectLoadedPieces()
	{
		var gameStateService = ServiceLocator.Get<IGameStateService>();
		SnapLoadedPieces(gameStateService);
		ConnectLoadedPieces(gameStateService);
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

		_piecesWithinSnappingDistance.Clear();
		_pieceBeingDragged = new IntVector2(-1, -1);

		_gameStateService.UpdatePositionAndSaveConnections(piece, this);
		
		if (_gameStateService.HasWon) { OnPlayerHasWon(); }
	}

	public Vector3 GetWorldPositionForPiece(IntVector2 boardPosition)
	{
		var piece = FindPiece(boardPosition);
		return piece ? piece.transform.position : Vector3.zero;
	}

	public void MarkPieceOnPlayField(PiecePrefab piece)
	{
		_piecesOnBoard.Add(piece);
		_gameStateService.AddToPlayField(piece.BoardPosition, piece.transform.position);
	}

	public void MarkPieceInPool(PiecePrefab piece)
	{
		_piecesOnBoard.Remove(piece);
		_gameStateService.RemoveFromPlayField(piece.BoardPosition);
	}

	public void ShowPiecesInSnappingDistanceTo(List<PiecePrefab> piecesBeingDragged, IntVector2 pieceBeingMovedPosition)
	{
		if (pieceBeingMovedPosition != _pieceBeingDragged)
		{
			// When looking for snappable pieces, we compare the position of one set of
			// connected pieces (that are being moved) to that of every other piece. But
			// we don't need to recalculate all the time what "every other piece" means;
			// so we cache it and only recalculate when the user starts a new drag.

			FindPiecesNotBeingDraggedRightNow(piecesBeingDragged);
			_pieceBeingDragged = pieceBeingMovedPosition;
		}

		foreach (var piece in _piecesWithinSnappingDistance)
		{
			piece.ScaleToNormalSize();
			piece.HideHighlight();
		}

		_piecesWithinSnappingDistance.Clear();

		var piecesDirectlyTouchingMovedPiece = GetPiecesNearDraggedOnes(piecesBeingDragged);

		foreach (var piece in piecesDirectlyTouchingMovedPiece)
		{
			_piecesWithinSnappingDistance.Add(piece);
		}

		HighlightPiecesInSnappingDistance(piecesBeingDragged, piecesDirectlyTouchingMovedPiece);
	}

	public void Shutdown()
	{
		OnPlayerHasWon = null;
		_gameStateService = null;
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

	private void SnapToPieceAt(PiecePrefab pieceToKeepSteady, PiecePrefab pieceToMove)
	{
		var positionDifference = pieceToMove.BoardPosition - pieceToKeepSteady.BoardPosition;

		var delta = new Vector3(positionDifference.x * BoardContext.PieceWidthInWorldUnits,
								positionDifference.y * BoardContext.PieceHeightInWorldUnits);

		pieceToMove.MoveTo(pieceToKeepSteady.transform.position + delta);
	}

	private void ConnectPieces(PiecePrefab a, PiecePrefab b)
	{
		a.ConnectTo(b);
		b.ConnectTo(a);
	}

	private void SnapLoadedPieces(IGameStateService gameStateService)
	{
		foreach (var root in gameStateService.Roots)
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
	}

	private void ConnectLoadedPieces(IGameStateService gameStateService)
	{
		foreach (var root in gameStateService.Roots)
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

	private void FindPiecesNotBeingDraggedRightNow(List<PiecePrefab> piecesBeingDragged)
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

	private List<PiecePrefab> GetPiecesNearDraggedOnes(List<PiecePrefab> piecesBeingDragged)
	{
		var piecesNearBy = new List<PiecePrefab>();

		foreach (var draggedPiece in piecesBeingDragged)
		{
			foreach (var pieceOnBoard in _piecesNotBeingDraggedRightNow)
			{
				if (!piecesNearBy.Contains(pieceOnBoard) &&
					AdjacencyHelper.IsWithinSnappingDistance(draggedPiece, pieceOnBoard))
				{
					piecesNearBy.AddRange(pieceOnBoard.connectedPieces);
				}
			}
		}

		return piecesNearBy;
	}

	private void HighlightPiecesInSnappingDistance(List<PiecePrefab> piecesBeingDragged, List<PiecePrefab> piecesToHighlight)
	{
		if (piecesToHighlight.Count < 1) { return; }

		// piece background should go above all the pieces that are being moved,
		// or being highlighted. => e.g. if 7 pieces are moved or highlighted, then
		// background should go to [length - 7 - 1]
		// highlighted pieces should go to an index after that,
		// and then moved pieces accordingly.

		var totalSiblingCount = _puzzleContainer.childCount;
		var siblingIndex = totalSiblingCount - piecesBeingDragged.Count - piecesToHighlight.Count;

		_pieceOutlineDisplay.SetSiblingIndex(siblingIndex++);

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
}
