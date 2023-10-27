using System;
using System.Linq;
using Godot;
using Rudzoft.ChessLib;
using Rudzoft.ChessLib.MoveGeneration;
using Rudzoft.ChessLib.Types;

class OpponentStupid : IOpponent
{
    private bool currentTurn;
    private bool isWhite;

    public OpponentStupid(bool isWhite)
    {
        currentTurn = isWhite;
        this.isWhite = isWhite;
    }

    private Move GetMove(Position pos)
    {
        MoveList moves = pos.GenerateMoves();
        
        return moves[new Random().Next(moves.Length)];
    }


    public bool IsTurn()
    {
        return currentTurn;
    }

    public void FlipCol()
    {
        isWhite = !isWhite;
        currentTurn = !currentTurn;
    }

    public void BeginPonder()
    {
        currentTurn = true;
        GD.Print("Pondering");

        ChessLogic.SubmitComputerMove(GetMove(ChessLogic.GetPos()));
        
        EndPonder();
    }

    private void EndPonder()
    {
        currentTurn = false;
    }

}