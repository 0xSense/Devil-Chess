using System;
using System.Linq;
using Godot;
using Rudzoft.ChessLib;
using Rudzoft.ChessLib.MoveGeneration;
using Rudzoft.ChessLib.Types;

class OpponentStupid : IOpponent
{
    public Move GetMove(IPosition pos)
    {
        var moveList = pos.GenerateMoves();

        GD.Print(moveList.Count());

        return moveList[(new Random()).Next(0, moveList.Length)];
    }
}