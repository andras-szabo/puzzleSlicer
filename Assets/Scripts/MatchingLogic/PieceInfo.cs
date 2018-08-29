public class PieceInfo
{
	public PieceInfo(int x, int y)
	{
		posX = x;
		posY = y;
	}

	public readonly int posX, posY;

	public Connection edgeLeft = Connection.None;
	public Connection edgeTop = Connection.None;
	public Connection edgeRight = Connection.None;
	public Connection edgeBottom = Connection.None;
}

