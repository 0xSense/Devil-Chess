using Rudzoft.ChessLib;
using Rudzoft.ChessLib.Types;

public class HumanPlayer : IPlayer
{
	private Move queuedMove;
	private bool isWhite;
	private bool currentTurn;
	public bool CurrentTurn => currentTurn;

	public HumanPlayer(bool isWhite)
	{
		this.isWhite = isWhite;
		currentTurn = isWhite;
	}

	public void SetMove(Move move)
	{
		this.queuedMove = move;
	}

	public void FlipCol()
	{
		isWhite = !isWhite;
		currentTurn = !currentTurn;
	}

	public void BeginWait()
	{
		currentTurn = false;
	}
}
