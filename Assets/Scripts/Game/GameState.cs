using System.Collections.Generic;

public class GameState
{
	public Dictionary<IntVector2, RootInfo> rootsByBoardPosition = new Dictionary<IntVector2, RootInfo>();
	public Dictionary<IntVector2, IntVector2> rootPositionsForEachPiece = new Dictionary<IntVector2, IntVector2>();
	public int _totalPieceCount = -1;

	public override string ToString()
	{
		return string.Format("[GameState] Root Count: {0}", rootsByBoardPosition.Count);
	}
}