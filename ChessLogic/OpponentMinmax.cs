using System;
using Godot;
using Rudzoft.ChessLib;
using Rudzoft.ChessLib.MoveGeneration;
using Rudzoft.ChessLib.Types;

public class OpponentMinmax : IOpponent
{
    private bool currentTurn;
    private bool isWhite;

    public OpponentMinmax(bool isWhite)
    {
        currentTurn = isWhite;
        this.isWhite = isWhite;
    }

    private Move GetMove(Position pos)
    {
        MoveList moves = pos.GenerateMoves();
        
        return moves[new Random().Next(moves.Length)];
    }

    public void BeginPonder()
    {
        currentTurn = true;
        GD.Print("Pondering");

        ChessLogic.SubmitComputerMove(GetMove(ChessLogic.GetPos()));
        
        EndPonder();
    }

    public void FlipCol()
    {
        isWhite = !isWhite;
        currentTurn = !currentTurn;
    }

    public bool IsTurn()
    {
        return currentTurn;
    }

    private void EndPonder()
    {
        currentTurn = false;
    }
}