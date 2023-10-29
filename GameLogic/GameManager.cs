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

        pieces = new PieceMovement[34];
        int i = 0;

        // the horrorshow continues . . .
        pawns = new int[16];
        knights = new int[4];
        bishops = new int[4];
        rooks = new int[4];
        queens = new int[4];
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
                case "W_Queen2":
                queens[1] = i;
                break;
                case "B_Queen":
                queens[2] = i;
                break;
                case "B_Queen2":
                queens[3] = i;
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
        ChessLogic.NewGame(true, "Pnbqkbnr/prpppppp/p/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1");
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
                    //GD.Print("Invalid move or not your turn.");
                }
                queuedSquare = NULL_SQUARE;
            }
        }
        else
            queuedSquare = algebraic;

        if (state.GameOver) {
            if(state.BlackChecked) {
                GD.Print("YOU WON!!!");
                var scene = GD.Load<PackedScene>("res://game_won.tscn"); 
                GetTree().ChangeSceneToPacked(scene);
            } else if(state.WhiteChecked) {
                GD.Print("GAME OVER!!!");
                var scene = GD.Load<PackedScene>("res://game_over.tscn"); 
                GetTree().ChangeSceneToPacked(scene);
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


        if (state.GameOver) {
            if(state.BlackChecked) {
                GD.Print("YOU WON!!!");
                var scene = GD.Load<PackedScene>("res://game_won.tscn"); 
                GetTree().ChangeSceneToPacked(scene);
            } else if(state.WhiteChecked) {
                GD.Print("GAME OVER!!!");
                var scene = GD.Load<PackedScene>("res://game_over.tscn"); 
                GetTree().ChangeSceneToPacked(scene);
            }
        }
    }

    private void RefreshBoard()
    {
        if (state.HumanIsWhite == state.WhiteToMove)
            GetNode<Label3D>("Label3D").Text = "Player's Move";
        else
            GetNode<Label3D>("Label3D").Text = "Enemy's Move";

        int bPawnCount = 0;
        int wPawnCount = 0;
        int bKnightCount = 0;
        int wKnightCount = 0;
        int bBishopCount = 0;
        int wBishopCount = 0;
        int bRookCount = 0;
        int wRookCount = 0;
        int bQueenCount = 0;
        int wQueenCount = 0;

        foreach (PieceMovement piece in pieces)
        {
            piece.Hide();
        }

        for (int col = 0; col < 8; col++)
        {
            for (int row = 0; row < 8; row++)
            {
                char piece = state.board[7-row,col];

                PieceMovement pMovement;

                Vector2I pos = new(col, row);

                switch (piece)
                {
                    case 'K':
                    pMovement = pieces[kings[0]];                     
                    break;
                    case 'Q':
                    //pMovement = pieces[queens[0]];
                    pMovement = pieces[queens[wQueenCount++]];
                    break;
                    case 'R':
                    pMovement = pieces[rooks[wRookCount++]];
                    break;
                    case 'B':
                    pMovement = pieces[bishops[wBishopCount++]];
                    break;
                    case 'N':
                    pMovement = pieces[knights[wKnightCount++]];
                    break;
                    case 'P':
                    pMovement = pieces[pawns[wPawnCount++]];
                    break;

                    case 'k':
                    pMovement = pieces[kings[1]];
                    break;
                    case 'q':
                    //pMovement = pieces[queens[1]];
                    pMovement = pieces[queens[2+bQueenCount++]];
                    break;
                    case 'r':
                    pMovement = pieces[rooks[2+bRookCount++]];
                    break;
                    case 'b':
                    pMovement = pieces[bishops[2+bBishopCount++]];
                    break;
                    case 'n':
                    pMovement = pieces[knights[2+bKnightCount++]];
                    break;
                    case 'p':
                    pMovement = pieces[pawns[8+bPawnCount++]];
                    break;
                    case ' ':
                    pMovement = null;
                    break;

                    default:
                    pMovement = null;
                    //GD.Print("Invalid character: ", piece);
                    break;
                }
                pMovement?.MoveToChessPosition(pos);
                pMovement?.Show();
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
