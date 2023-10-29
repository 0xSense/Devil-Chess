using System;
using Godot;
using Rudzoft.ChessLib;
using Rudzoft.ChessLib.MoveGeneration;
using Rudzoft.ChessLib.Types;
using System.Threading;
using Microsoft.VisualBasic;
using System.Runtime.CompilerServices;
using Rudzoft.ChessLib.Enums;

struct MoveEvalPair
{
    public float eval;
    public Move move;

    public MoveEvalPair(float e, Move m)
    {
        eval = e;
        move = m;
    }

    public MoveEvalPair()
    {
        eval = 0f;
        move = Move.EmptyMove;
    }
}

static class LeafValues
{
    public const float KingValue = float.PositiveInfinity;
    public const float QueenValue = 9.25f;
    public const float RookValue = 5f;
    public const float BishopValue = 3.3f;
    public const float KnightValue = 3f;
    public const float PawnValue = 1f;

}

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

        void ThreadStart()
        {
            SearchAndSubmit(ChessLogic.GetPos(), this.isWhite);
        }
        new Thread(ThreadStart).Start();    
    }

    private float EvalLeaf(Position pos)
    {
        // Handles checkmate & stalemate
        if (pos.IsMate)
        {
            if (!pos.InCheck)
                return 0.0f;
            if (pos.SideToMove == Player.White)
            {
                return float.NegativeInfinity;
            }
            else
            {
                return float.PositiveInfinity;
            }
        }

        int colorMultiplier = 1;
        float eval = 0.0f;

        // Standard eval
        foreach (Piece p in pos.Board)
        {
            
            colorMultiplier = p.IsWhite ? 1 : -1;
            switch (p.Type())
            {
                case PieceTypes.Queen:
                    eval += LeafValues.QueenValue * colorMultiplier;
                    break;
                case PieceTypes.Rook:
                    eval += LeafValues.RookValue * colorMultiplier;
                    break;
                case PieceTypes.Bishop:
                    eval += LeafValues.BishopValue * colorMultiplier;
                    break;
                case PieceTypes.Knight:
                    eval += LeafValues.KnightValue * colorMultiplier;
                    break;
                case PieceTypes.Pawn:
                    eval += LeafValues.PawnValue * colorMultiplier;
                    break;                
                default:
                    break;
            }
        }

        return eval;
    }

    private MoveEvalPair Minmax(Position pos, int depth, float alpha, float beta, bool maximizing, Move lastMove)
    {
        float eval;

        if (depth == 0)
        {
            MoveEvalPair pair = new MoveEvalPair(EvalLeaf(pos), lastMove);
            eval = pair.eval;
            return pair;
        }
        if (maximizing)
        {
            eval = float.NegativeInfinity;
            MoveList moves = pos.GenerateMoves(); // SLOW - allocates memory; change this to be in place?
            MoveEvalPair pair;
            MoveEvalPair bestPair = new MoveEvalPair();
            bestPair.eval = eval;

            for (int i = 0; i < moves.Length; i++)
            {
                pos.MakeMove(moves[i], new State());
                pair = Minmax(pos, depth-1, alpha, beta, !maximizing, moves[i]);
                pos.TakeMove(moves[i]);

                if (pair.eval > eval)
                {
                    eval = pair.eval;
                    bestPair = pair;
                    bestPair.move = moves[i];
                }                                

                if (eval >= beta)
                    break;
                alpha = Mathf.Max(alpha, eval);
            }
            return bestPair;
        }
        else
        {
            eval = float.PositiveInfinity;
            MoveList moves = pos.GenerateMoves(); // SLOW - allocates memory; change this to be in place?
            MoveEvalPair pair;
            MoveEvalPair bestPair = new MoveEvalPair();
            bestPair.eval = eval;

            for (int i = 0; i < moves.Length; i++)
            {
                pos.MakeMove(moves[i], new State());
                pair = Minmax(pos, depth-1, alpha, beta, !maximizing, moves[i]);
                pos.TakeMove(moves[i]);

                if (pair.eval < eval)
                {
                    eval = pair.eval;
                    bestPair = pair;
                    bestPair.move = moves[i];
                }

                if (eval <= alpha)
                    break;
                beta = Mathf.Min(beta, eval);
            }
            return bestPair;
        }
    }

    private void SearchAndSubmit(Position pos, bool isWhite)
    {
        // TODO: Iterative deepening
        Position currentPos = new Position(new Board(), new PieceValue());
        currentPos.Set(pos.GenerateFen(), ChessMode.Normal, new State(), true);
        
        System.Diagnostics.Stopwatch timer = new();
        timer.Start();
        MoveEvalPair result = Minmax(currentPos, 5, float.NegativeInfinity, float.PositiveInfinity, isWhite, Move.EmptyMove);
        timer.Stop();
        
        Move move = result.move;

        GD.Print("------\n");
        GD.Print("Calculation time: " + timer.ElapsedMilliseconds / 1000f);
        GD.Print("Eval: " + result.eval);
        GD.Print("Move: ", move.ToString());
        GD.Print("------\n");

        ChessLogic.SubmitComputerMove(move);

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