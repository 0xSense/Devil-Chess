using Godot;
using System;
using System.Diagnostics;
using System.Numerics;
using System.Runtime.InteropServices;

public partial class GameManager : Node
{
    private const float SQUARE_DIST = 0.092f - -0.096f; // Approximate distance between squares (may need to adjust?)
    private GameState state;
    private Vector2I queuedSquare;
    private readonly Vector2I NULL_SQUARE = new Vector2I(-1, -1);

    private PieceMovement[] pieces;

    // This is a hideous hack conveived in a state of sleep deprivation. I'm sorry.
    private int[] pawns;
    private int[] knights;
    private int[] bishops;
    private int[] rooks;
    private int[] queens;
    private int[] kings;
    public override void _Ready()
    {
        GetNode<LoadSignaller>("/root/game_primary").SceneLoaded += OnLoad;
        queuedSquare = NULL_SQUARE;
    }

    public void OnLoad()
    {
        GetNode<HighlightedSquare>("/root/game_primary/HighlightedSquare").SquareClicked += HandleClick;
        Node3D piecesNode = GetNode<Node3D>("/root/game_primary/Pieces");

        pieces = new PieceMovement[32];
        int i = 0;

        // the horrorshow continues . . .
        pawns = new int[16];
        knights = new int[4];
        bishops = new int[4];
        rooks = new int[4];
        queens = new int[2];
        kings = new int[2];

        int wPawnCount = 0;
        int bPawnCount = 0;

        foreach (PieceMovement node in piecesNode.GetChildren())
        {
            pieces[i] = node;
            

            GD.Print(node.Name.ToString() + "; " + i);

            // make everything dependant on strings, why not . . .

            if (node.Name.ToString().StartsWith("W_Pawn"))
            {
                //pawns[(int)Char.GetNumericValue(node.Name.ToString()[6])-1] = i;
                pawns[wPawnCount] = i;
                wPawnCount++;
                i++;
                continue;
            }

            else if (node.Name.ToString().StartsWith("B_Pawn"))
            {
                //pawns[7 + (int)Char.GetNumericValue(node.Name.ToString()[6])] = i;
                pawns[bPawnCount+8] = i;
                bPawnCount++;
                i++;
                continue;
            }


            switch (node.Name.ToString())
            {
                case "W_King": 
                kings[0] = i;
                break;
                case "B_King":
                kings[1] = i;
                break;
                case "W_Queen":
                queens[0] = i;
                break;
                case "B_Queen":
                queens[1] = i;
                break;
                case "W_RRook":
                rooks[0] = i;
                break;
                case "W_LRook":
                rooks[1] = i;
                break;
                case "B_RRook":
                rooks[2] = i;
                break;
                case "B_LRook":
                rooks[3] = i;
                break;
                case "W_LSB":
                bishops[0] = i;
                break;
                case "W_DSB":
                bishops[1] = i;
                break;
                case "B_LSB":
                bishops[2] = i;
                break;
                case "B_DSB":
                bishops[3] = i;
                break;
                case "W_RKnight":
                knights[0] = i;
                break;
                case "W_LKnight":
                knights[1] = i;
                break;
                case "B_RKnight":
                knights[2] = i;
                break;
                case "B_LKnight":
                knights[3] = i;
                break;                


                default:
                GD.Print("MISNAME DETECTED");
                throw new Exception("MISNAME DETECTED");
            }

            ++i;
        }

        void printList(int[] list)
        {
            foreach (int i in list)
            {
                GD.Print("Knights[i]: " + i + "; " + pieces[i].Name.ToString());
            }
            GD.Print();
        }

        printList(knights);
        //GD.Print(pawns[0]);

        ChessLogic.AIMoveFinished += OnAIMove;
        ChessLogic.NewGame(true);
        state = ChessLogic.GetBoardState();
    }

    public void HandleClick(Vector2I square)
    {
        Vector2I algebraic = ToAlgebraicCoords(square);
        GD.Print("Algebraic: ", algebraic);
        GD.Print("Internal: ", square);
        if (state.WhiteToMove == state.HumanIsWhite)
        {
            if (queuedSquare == NULL_SQUARE)
            {
                queuedSquare = algebraic;
            }
            else
            {
                if (ChessLogic.SubmitHumanMove(queuedSquare.X, queuedSquare.Y, algebraic.X, algebraic.Y))
                {
                    state = ChessLogic.GetBoardState();
                    //GetPieceOnSquare(queuedSquare)?.MoveToChessPosition(square);
                    GD.Print("Character: ", state.board[0,0]);
                    RefreshBoard();
                }
                else
                {
                    GD.Print("Invalid move or not your turn.");
                }
                queuedSquare = NULL_SQUARE;
            }
        }
    }

    public void OnAIMove(SimpleMove move)
    {
        state = ChessLogic.GetBoardState();
        RefreshBoard();
        /*
        GetPieceOnSquare(
            new Vector2I(move.fromCol, move.fromRow))?.MoveToChessPosition(
                FromAlgebraicCoords(new Vector2I(move.toCol, move.toRow)));*/
    }

    private void RefreshBoard()
    {
        int bPawnCount = 0;
        int wPawnCount = 0;
        int bKnightCount = 0;
        int wKnightCount = 0;
        int bBishopCount = 0;
        int wBishopCount = 0;
        int bRookCount = 0;
        int wRookCount = 0;

        for (int col = 0; col < 8; col++)
        {
            for (int row = 0; row < 8; row++)
            {
                char piece = state.board[7-row,col];

                Vector2I pos = new(col, row);

                switch (piece)
                {
                    case 'K':
                    pieces[kings[0]].MoveToChessPosition(pos);                    
                    break;
                    case 'Q':
                    pieces[queens[0]].MoveToChessPosition(pos);
                    break;
                    case 'R':
                    pieces[rooks[wRookCount++]].MoveToChessPosition(pos);
                    break;
                    case 'B':
                    pieces[bishops[wBishopCount++]].MoveToChessPosition(pos);
                    break;
                    case 'N':
                    pieces[knights[wKnightCount++]].MoveToChessPosition(pos);
                    break;
                    case 'P':
                    pieces[pawns[wPawnCount++]].MoveToChessPosition(pos);
                    break;

                    case 'k':
                    pieces[kings[1]].MoveToChessPosition(pos);
                    break;
                    case 'q':
                    pieces[queens[1]].MoveToChessPosition(pos);
                    break;
                    case 'r':
                    pieces[rooks[2+bRookCount++]].MoveToChessPosition(pos);
                    break;
                    case 'b':
                    pieces[bishops[2+bBishopCount++]].MoveToChessPosition(pos);
                    break;
                    case 'n':
                    pieces[knights[2+bKnightCount++]].MoveToChessPosition(pos);
                    break;
                    case 'p':
                    pieces[pawns[8+bPawnCount++]].MoveToChessPosition(pos);
                    break;
                    case ' ':
                    break;

                    default:
                    //GD.Print("Invalid character: ", piece);
                    break;
                }
            }
        }
    }

    private Vector2I ToAlgebraicCoords(Vector2I v)
    {
        return new Vector2I(v.X, 7-v.Y);
    }

    private Vector2I FromAlgebraicCoords(Vector2I v)
    {
        return new Vector2I(v.X, 7-v.Y);
    }

    // Takes algebraic coords in
    private PieceMovement GetPieceOnSquare(Vector2I square)
    {
        GD.Print("Getting piece: ", square);
        Vector2I algebraic = FromAlgebraicCoords(square);
        foreach (PieceMovement n in pieces)
        {
            if (n.ChessPosition == algebraic)
            {
                return n;
            }
        }

        GD.Print("Null");

        return null;
    }
}
