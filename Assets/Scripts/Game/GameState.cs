using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameState
{
	public const string GAME_STATE_PATH = "gameState.json";

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
	}

	private Dictionary<IntVector2, RootInfo> _rootsByBoardPosition = new Dictionary<IntVector2, RootInfo>();
	private Dictionary<IntVector2, IntVector2> _rootPositionsForEachPiece = new Dictionary<IntVector2, IntVector2>();

	public IEnumerable<RootInfo> Roots
	{
		get
		{
			return _rootsByBoardPosition.Values;
		}
	}

	public override string ToString()
	{
		return string.Format("[GameState] Root Count: {0}", _rootsByBoardPosition.Count);
	}

	public bool IsOnPlayField(int col, int row)
	{
		return _rootPositionsForEachPiece.ContainsKey(new IntVector2(col, row));
	}

	public bool TryLoad()
	{
		var path = System.IO.Path.Combine(Application.persistentDataPath, GAME_STATE_PATH);
		var success = false;
		try
		{
			var asJson = System.IO.File.ReadAllText(path);
			var serialized = JsonUtility.FromJson<SerializedGameState>(asJson);
			Deserialize(serialized);
			success = true;
		}
		catch (Exception e)
		{
			Debug.LogWarning(e.Message);
		}

		return success;
	}

	private void Deserialize(SerializedGameState sgs)
	{
		_rootPositionsForEachPiece.Clear();
		_rootsByBoardPosition.Clear();

		foreach (var root in sgs.roots)
		{
			_rootsByBoardPosition.Add(root.boardPosition, root);
			foreach (var piece in root.connectedBoardPositions)
			{
				_rootPositionsForEachPiece[piece] = root.boardPosition;
			}
		}
	}

	public Vector3 GetWorldPosition(int col, int row)
	{
		var boardPos = new IntVector2(col, row);

		IntVector2 rootPos;
		if (_rootPositionsForEachPiece.TryGetValue(boardPos, out rootPos))
		{
			return _rootsByBoardPosition[rootPos].GetWorldPosition(boardPos);
		}

		return Vector3.zero;
	}

	public void Save()
	{
		var serializedState = new SerializedGameState 
		{ 
			roots = new List<RootInfo>(_rootsByBoardPosition.Values) 
		};

		var asJson = JsonUtility.ToJson(serializedState);
		var path = System.IO.Path.Combine(Application.persistentDataPath, GAME_STATE_PATH);

		try
		{
			System.IO.File.WriteAllText(path, asJson);
		}
		catch (System.Exception e)
		{
			Debug.LogWarning(e.Message);
		}
	}

	public void Clear()
	{
		_rootsByBoardPosition.Clear();
		_rootPositionsForEachPiece.Clear();
	}

	public void AddToPlayField(IntVector2 boardPosition, Vector3 worldPosition)
	{
		if (_rootPositionsForEachPiece.ContainsKey(boardPosition))
		{
			return;
		}

		var newRoot = new RootInfo
		{
			worldPosition = worldPosition,
			boardPosition = boardPosition,
			connectedBoardPositions = new List<IntVector2> { boardPosition }
		};

		_rootsByBoardPosition.Add(boardPosition, newRoot);
		_rootPositionsForEachPiece.Add(boardPosition, boardPosition);
	}

	public void RemoveFromPlayField(IntVector2 boardPosition)
	{
		_rootsByBoardPosition.Remove(boardPosition);
		_rootPositionsForEachPiece.Remove(boardPosition);
	}

	public void UpdatePositionAndSaveConnections(PiecePrefab piece)
	{
		var uniqueRootPositions = new List<IntVector2>();

		foreach (var connectedPiece in piece.connectedPieces)
		{
			var rootPositionForPiece = _rootPositionsForEachPiece[connectedPiece.BoardPosition];

			if (!uniqueRootPositions.Contains(rootPositionForPiece))
			{
				uniqueRootPositions.Add(rootPositionForPiece);
			}
		}

		for (int i = 1; i < uniqueRootPositions.Count; ++i)
		{
			MergeRoots(uniqueRootPositions[0], uniqueRootPositions[i]);
		}

		var rootPosition = _rootPositionsForEachPiece[piece.BoardPosition];
		_rootsByBoardPosition[rootPosition].worldPosition = PuzzleService.Instance.GetWorldPositionForPiece(rootPosition);
	}

	private void MergeRoots(IntVector2 a, IntVector2 b)
	{
		var rootA = _rootsByBoardPosition[a];
		var rootB = _rootsByBoardPosition[b];

		if (rootA == rootB)
		{
			Debug.LogWarning("this shouldn't happen");
			return;
		}

		rootA.connectedBoardPositions.AddRange(rootB.connectedBoardPositions);

		foreach (var connectedPiece in rootB.connectedBoardPositions)
		{
			_rootPositionsForEachPiece[connectedPiece] = a;
		}

		_rootsByBoardPosition.Remove(b);
	}
}