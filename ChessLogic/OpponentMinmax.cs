using System;
using Godot;
using Rudzoft.ChessLib;
using Rudzoft.ChessLib.MoveGeneration;
using Rudzoft.ChessLib.Types;
using System.Threading;
using Microsoft.VisualBasic;
using System.Runtime.CompilerServices;
using Rudzoft.ChessLib.Enums;
using System.Collections;
using System.Collections.Generic;

struct MoveEvalPair
{
    public int eval;
    public Move move;

    public MoveEvalPair(int e, Move m)
    {
        eval = e;
        move = m;
    }

    public MoveEvalPair()
    {
        eval = 0;
        move = Move.EmptyMove;
    }
}

static class LeafValues
{
    public const int KingValue = 0;
    public const int QueenValue = 925;
    public const int RookValue = 500;
    public const int BishopValue = 330;
    public const int KnightValue = 300;
    public const int PawnValue = 100;

    

}

public unsafe class OpponentMinmax : IOpponent
{
    private bool currentTurn;
    private bool isWhite;

    private static Dictionary<PieceTypes, int> pieceValues = new();
    private static int[] pieceValuesArray;

    public OpponentMinmax(bool isWhite)
    {
        currentTurn = isWhite;
        this.isWhite = isWhite;

        pieceValues.Clear();
        pieceValues.Add(PieceTypes.Queen, LeafValues.QueenValue);
        pieceValues.Add(PieceTypes.Rook, LeafValues.RookValue);
        pieceValues.Add(PieceTypes.Bishop, LeafValues.BishopValue);
        pieceValues.Add(PieceTypes.Knight, LeafValues.KnightValue);
        pieceValues.Add(PieceTypes.Pawn, LeafValues.PawnValue);
        pieceValues.Add(PieceTypes.King, LeafValues.KingValue);

        pieceValuesArray = new int[7];
        pieceValuesArray[(int)PieceTypes.Queen] = LeafValues.QueenValue;
        pieceValuesArray[(int)PieceTypes.Rook] = LeafValues.RookValue;
        pieceValuesArray[(int)PieceTypes.Bishop] = LeafValues.BishopValue;
        pieceValuesArray[(int)PieceTypes.Knight] = LeafValues.KnightValue;
        pieceValuesArray[(int)PieceTypes.Pawn] = LeafValues.PawnValue;
        pieceValuesArray[(int)PieceTypes.King] = LeafValues.KingValue;

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

    // EvalLeaf() variables moved out of function for
    

    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private unsafe int EvalLeaf(Position pos)
    {
        
        // Handles checkmate & stalemate
        if (pos.IsMate)
        {
            if (!pos.InCheck)
                return 0;
            if (pos.SideToMove == Player.White)
            {
                return -100000;
            }
            else
            {
                return 100000;
            }
        }

        int colorMultiplier = 1;
        int eval = 0;

        Piece[] pieces = pos.Board.GetPieceArray();
        int count = pieces.Length;
        Piece p;
        int i = 0;

        bool white;
        bool black;

        // Standard eval
        while (i < count)
        {
            p = pieces[i];
            white = p.IsWhite;
            black = !white;
            colorMultiplier = (1 * (*(SByte*)&white)) + (-1 * (*(SByte*)&black)); 
            //colorMultiplier = p.IsWhite ? 1 : -1;

            /*
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
            }*/
            //eval += pieceValues.GetValueOrDefault(p.Type()) * colorMultiplier;
            eval += pieceValuesArray[(int)p.Type()] * colorMultiplier;
            i++;
        }

        return eval;
    }

    private MoveEvalPair Minmax(Position pos, int depth, int alpha, int beta, bool maximizing, Move lastMove)
    {
        int eval;

        if (depth == 0)
        {
            MoveEvalPair pair = new MoveEvalPair(EvalLeaf(pos), lastMove);
            eval = pair.eval;
            return pair;
        }
        if (maximizing)
        {
            eval = -100000; //float.NegativeInfinity;
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
            eval = 100000; //float.PositiveInfinity;
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
        MoveEvalPair result = Minmax(currentPos, 5, -100000, 100000, isWhite, Move.EmptyMove);
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