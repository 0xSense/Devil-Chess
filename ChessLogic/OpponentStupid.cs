using System;
using System.Linq;
using Godot;
using Rudzoft.ChessLib;
using Rudzoft.ChessLib.MoveGeneration;
using Rudzoft.ChessLib.Types;

class OpponentStupid : IOpponent
{
    private bool currentTurn;

    public OpponentStupid(bool isWhite)
    {
        currentTurn = isWhite;
    }

    public Move GetMove(IPosition pos)
    {
        var moveList = pos.GenerateMoves();

        return moveList[(new Random()).Next(0, moveList.Length)];
    }

    public bool IsTurn()
    {
        return currentTurn;
    }
}