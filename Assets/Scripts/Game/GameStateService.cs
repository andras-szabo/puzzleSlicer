using System;
using System.Collections.Generic;
using UnityEngine;

public interface IGameStateService : IService
{
	void Setup(int pieceCount);
	void Save();
	void AddToPlayField(IntVector2 boardPosition, Vector3 worldPosition);
	void RemoveFromPlayField(IntVector2 boardPosition);
	void UpdatePositionAndSaveConnections(PiecePrefab piece, IBoardService boardService);

	bool TryLoad(int expectedPieceCount);
	IEnumerable<RootInfo> Roots { get; }
	bool HasWon { get; }
	bool IsPieceOnPlayField(int col, int row);
	Vector3 GetWorldPosition(int col, int row);
}

[Serializable]
public class RootInfo
{
	public Vector3 worldPosition;
	public IntVector2 boardPosition;
	public List<IntVector2> connectedBoardPositions;

	public Vector3 GetWorldPosition(IntVector2 boardPosition)
	{
		var position = worldPosition;

		return position;
	}
}

[Serializable]
public class SerializedGameState
{
	public List<RootInfo> roots;
	public int totalPieceCount;
	public bool hasWon;
}

public class GameStateService : IGameStateService
{
	private GameState _gameState = new GameState();

	public IEnumerable<RootInfo> Roots
	{
		get
		{
			return _gameState.rootsByBoardPosition.Values;
		}
	}

	public bool HasWon { get; private set; }

	public bool IsPieceOnPlayField(int col, int row)
	{
		return _gameState.rootPositionsForEachPiece.ContainsKey(new IntVector2(col, row));
	}

	public Vector3 GetWorldPosition(int col, int row)
	{
		var boardPos = new IntVector2(col, row);

		IntVector2 rootPos;
		if (_gameState.rootPositionsForEachPiece.TryGetValue(boardPos, out rootPos))
		{
			return _gameState.rootsByBoardPosition[rootPos].GetWorldPosition(boardPos);
		}

		return Vector3.zero;
	}

	public void AddToPlayField(IntVector2 boardPosition, Vector3 worldPosition)
	{
		if (_gameState.rootPositionsForEachPiece.ContainsKey(boardPosition))
		{
			return;
		}

		var newRoot = new RootInfo
		{
			worldPosition = worldPosition,
			boardPosition = boardPosition,
			connectedBoardPositions = new List<IntVector2> { boardPosition }
		};

		_gameState.rootsByBoardPosition.Add(boardPosition, newRoot);
		_gameState.rootPositionsForEachPiece.Add(boardPosition, boardPosition);
	}

	public void RemoveFromPlayField(IntVector2 boardPosition)
	{
		_gameState.rootsByBoardPosition.Remove(boardPosition);
		_gameState.rootPositionsForEachPiece.Remove(boardPosition);
	}

	public void Init()
	{
		_gameState.rootsByBoardPosition.Clear();
		_gameState.rootPositionsForEachPiece.Clear();
		_gameState._totalPieceCount = -1;
		HasWon = false;
	}

	public void Save()
	{
		var serializedState = new SerializedGameState
		{
			roots = new List<RootInfo>(_gameState.rootsByBoardPosition.Values),
			totalPieceCount = _gameState._totalPieceCount,
			hasWon = HasWon
		};

		var asJson = JsonUtility.ToJson(serializedState);
		try
		{
			System.IO.File.WriteAllText(Paths.GetFullPathToGameState(), asJson);
		}
		catch (Exception e)
		{
			Debug.LogWarning(e.Message);
		}
	}

	public void Setup(int pieceCount)
	{
		_gameState._totalPieceCount = pieceCount;
		HasWon = false;
	}

	public void Shutdown()
	{
	}

	public bool TryLoad(int expectedPieceCount)
	{
		var success = false;
		try
		{
			var asJson = System.IO.File.ReadAllText(Paths.GetFullPathToGameState());
			var serialized = JsonUtility.FromJson<SerializedGameState>(asJson);
			Deserialize(serialized, expectedPieceCount);
			success = true;
		}
		catch (Exception e)
		{
			Debug.LogWarning(e.Message);
		}

		return success;
	}

	public void UpdatePositionAndSaveConnections(PiecePrefab piece, IBoardService boardService)
	{
		var uniqueRootPositions = new List<IntVector2>();

		foreach (var connectedPiece in piece.connectedPieces)
		{
			var rootPositionForPiece = _gameState.rootPositionsForEachPiece[connectedPiece.BoardPosition];

			if (!uniqueRootPositions.Contains(rootPositionForPiece))
			{
				uniqueRootPositions.Add(rootPositionForPiece);
			}
		}

		for (int i = 1; i < uniqueRootPositions.Count; ++i)
		{
			MergeRoots(uniqueRootPositions[0], uniqueRootPositions[i]);
		}

		var rootPosition = _gameState.rootPositionsForEachPiece[piece.BoardPosition];
		_gameState.rootsByBoardPosition[rootPosition].worldPosition = boardService.GetWorldPositionForPiece(rootPosition);

		CheckIfWon();
	}

	private void MergeRoots(IntVector2 a, IntVector2 b)
	{
		var rootA = _gameState.rootsByBoardPosition[a];
		var rootB = _gameState.rootsByBoardPosition[b];

		Debug.Assert(rootA != rootB, "[GameStateService:MergeRoots] Trying to merge same roots. This shouldn't happen.");

		rootA.connectedBoardPositions.AddRange(rootB.connectedBoardPositions);

		foreach (var connectedPiece in rootB.connectedBoardPositions)
		{
			_gameState.rootPositionsForEachPiece[connectedPiece] = a;
		}

		_gameState.rootsByBoardPosition.Remove(b);
	}

	private void Deserialize(SerializedGameState serializedGameState, 
							 int expectedTotalPieceCount)
	{
		_gameState.rootPositionsForEachPiece.Clear();
		_gameState.rootsByBoardPosition.Clear();

		foreach (var root in serializedGameState.roots)
		{
			_gameState.rootsByBoardPosition.Add(root.boardPosition, root);
			foreach (var piece in root.connectedBoardPositions)
			{
				_gameState.rootPositionsForEachPiece[piece] = root.boardPosition;
			}
		}

		if (serializedGameState.totalPieceCount == 0)                       // Dealing with legacy gamestate without total piece count
		{
			_gameState._totalPieceCount = expectedTotalPieceCount;
			CheckIfWon();
		}
		else
		{
			_gameState._totalPieceCount = serializedGameState.totalPieceCount;
			HasWon = serializedGameState.hasWon;
		}
	}

	private void CheckIfWon()
	{
		HasWon = _gameState.rootsByBoardPosition.Count == 1
			  && _gameState.rootPositionsForEachPiece.Count == _gameState._totalPieceCount;
	}
}