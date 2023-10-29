using Godot;
using Rudzoft.ChessLib;
using Rudzoft.ChessLib.Factories;
using Rudzoft.ChessLib.MoveGeneration;
using Rudzoft.ChessLib.Types;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection.Metadata;
using System.Runtime.CompilerServices;
using System.Threading;

/*

Quick overview of the code you need to worry about as a gameplay programmer:

GameState - struct representing the state of the chess game including piece positions;
SimpleMove - struct representing a single move (returned by ChessLogic.GetLegalMoves())
ChessLogic - Static class which contains all methods you may need to call:
	ChessLogic.ReverseColors() - Switches who is playing white and who is playing black (human defaults to white);
	ChessLogic.GetBoardState() -> GameState - Returns current state of the board as GameState instance;
	ChessLogic.SubmitHumanMove(int fromCol, int fromRow, int toCol, int toRow) - Call this to tell my code when the player has moved a piece in the game world. This function will only apply the move if it is legal and will return false otherwise.
	ChessLogic.GetLegalMoves() -> List<SimpleMove> - Returns all legal moves in current position. Useful for highlighting legal moves for player.
	ChessLogic.NewGame(bool humanIsWhite) - Starts new game. Call this before any other functions but *after* registering a function with the AIMoveFinished event.
	ChessLogic.AIMoveFinished - C# event which is invoked when the AI makes a move. Assign at least one callback before calling any other ChessLogic method.
*/

// Simple representation of a move based on to and from squares
public struct SimpleMove
{
    public int fromCol;
    public int fromRow;
    public int toCol;
    public int toRow;

    public SimpleMove(int fCol, int fRow, int tCol, int tRow)
    {
        fromCol = fCol;
        fromRow = fRow;
        toCol = tCol;
        toRow = tRow;
    }
}

public struct GameState
{
    // Represents board as 8x8 array. Each square's piece or lack thereof is represented by a modified algebraic notation equivalent: uppercase denotes a white piece, while lowercase denotes a black one. A space character denotes an empty square.
    /*
    Example of starting position:

    [
        ['r', 'n', 'b', 'q', 'k', 'b', 'n', 'r'],
        ['p', 'p', 'p', 'p', 'p', 'p', 'p', 'p'],
        [' ', ' ', ' ', ' ', ' ', ' ', ' ', ' '],
        [' ', ' ', ' ', ' ', ' ', ' ', ' ', ' '],
        [' ', ' ', ' ', ' ', ' ', ' ', ' ', ' '],
        [' ', ' ', ' ', ' ', ' ', ' ', ' ', ' '],
        ['P', 'P', 'P', 'P', 'P', 'P', 'P', 'P'],
        ['R', 'N', 'B', 'Q', 'K', 'B', 'N', 'R']
    ]

    */
    public char[,] board;
    // Whether white can still castle in the position
    public bool WhiteCastlingRights;
    // Whether black can still castle
    public bool BlackCastlingRights;
    // The square where the player to move can perform an en passant *into*. 0-63, moving left to right. -1 for no such square.
    public int EnPassantSquare;
    // Whether white is in check
    public bool WhiteChecked;
    // Same for black
    public bool BlackChecked;
    // True if game is over in any way. If game is over and white is in check, black has won by checkmate. If black is in check, white has one. If neither is in check, game has ended in stalemate (draw)
    public bool GameOver;
    // True if white is to move; false if black is.
    public bool WhiteToMove;
    public bool HumanIsWhite;

    public GameState()
    {
        board = new char[8, 8];
        WhiteCastlingRights = true;
        BlackCastlingRights = true;
        EnPassantSquare = -1;
        WhiteChecked = false;
        BlackChecked = false;
        GameOver = false;
        WhiteToMove = true;
        HumanIsWhite = true;
    }
}

public static class ChessLogic
{
    public delegate void AIMoveNotify(SimpleMove move);
    public static event AIMoveNotify AIMoveFinished;

    private static IPlayer human;
    private static IPlayer AI;

    private static bool humanIsWhite;

    const char W_KING = 'K';
    const char W_QUEEN = 'Q';
    const char W_BISHOP = 'B';
    const char W_KNIGHT = 'N';
    const char W_ROOK = 'R';
    const char W_PAWN = 'P';

    const char B_KING = 'k';
    const char B_QUEEN = 'q';
    const char B_BISHOP = 'b';
    const char B_KNIGHT = 'n';
    const char B_ROOK = 'r';
    const char B_PAWN = 'p';
    const char SQUARE_EMPTY = ' ';

    private static GameState readableState;

    private static Game game;

    public static void NewGame(bool humanIsWhite)
    {
        if (AIMoveFinished == null)
        {
            throw new Exception("ERROR: You must subscribe a function to AIMoveNotify event before starting a new game.");
        }
        else
        {
            readableState = new GameState();
            game = (Game)GameFactory.Create("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1");        
        }

        human = new HumanPlayer(humanIsWhite);
        AI = new OpponentMinmax(!humanIsWhite);
        ChessLogic.humanIsWhite = humanIsWhite;
    }

    public static void NewGame(bool humanIsWhite, String fen)
    {
        if (AIMoveFinished == null)
        {
            throw new Exception("ERROR: You must subscribe a function to AIMoveNotify event before starting a new game.");
        }
        else
        {
            readableState = new GameState();
            game = (Game)GameFactory.Create(fen);        
        }

        human = new HumanPlayer(humanIsWhite);
        AI = new OpponentMinmax(!humanIsWhite);
    }

    // By default, the human plays white; this function inverts that.
    public static void ReverseColors()
    {
        human.FlipCol();
        AI.FlipCol();
    }

    public static List<SimpleMove> GetLegalMoves()
    {
        List<SimpleMove> moves = new();

        foreach (Move m in game.Pos.GenerateMoves())
        {
            Square fromSquare = m.FromSquare();
            Square toSquare = m.ToSquare();
            
            moves.Add(new SimpleMove(fromSquare.File.AsInt(), fromSquare.Rank.AsInt(), toSquare.File.AsInt(), toSquare.Rank.AsInt()));
        }

        return moves;
    }

    /**
    Returns a struct with all game information needed to visually represent the board in game.
    */
    public static GameState GetBoardState()
    {
        return readableState;
    }

    // Do not call this function.
    public static Position GetPos()
    {
        return (Position)game.Pos;
    }

    /*
     Call this function when the player has moved a piece in the game world to inform my code of it.
     Takes two ints for the square the piece moves from and two ints for the square the piece moves to (__Col -> x, __Row -> y).
     This will return false and do nothing if the move is illegal or if it is not the human player's turn.
     Zero-indexed.
    */
    public static bool SubmitHumanMove(int fromCol, int fromRow, int toCol, int toRow)
    {
        if (!((HumanPlayer)human).CurrentTurn)
        {
            GD.Print("Not your turn.");
            return false;
        }

        Square from, to;
        from = new Square(fromRow, fromCol);
        to = new Square(toRow, toCol);
        Move move = new Move(from, to);

        MoveList legalMoves = game.Pos.GenerateMoves();


        foreach (Move testMove in legalMoves)
        {
            if (testMove.Equals(move) || compareMoveSquaresByStrings(testMove, move))
            {
                MakeMove(testMove);
                ((IOpponent)AI).BeginPonder();
                ((HumanPlayer)human).BeginWait();
                return true;
            }
            else
            {
                //GD.Print(move.FromSquare(), "; ", move.ToSquare());
            }
        }        
        
        GD.Print("Invalid move");
        return false;
    }

    // Do not attempt to call this function from gameplay code.
    public static void SubmitComputerMove(Move move)
    {
        ((HumanPlayer)human).EndWait();
        MakeMove(move);
        AIMoveFinished?.Invoke(new SimpleMove(move.FromSquare().File.AsInt(), move.FromSquare().Rank.AsInt(),
             move.ToSquare().File.AsInt(), move.ToSquare().Rank.AsInt()));

        //PrintMovesDebug();
    }

    private static bool compareMoveSquaresByStrings(Move m1, Move m2)
    {
        return
            m1.FromSquare().ToString().Equals(m2.FromSquare().ToString()) &&
            m1.ToSquare().ToString().Equals(m2.ToSquare().ToString())
        ;
    }

    private static void PrintMovesDebug()
    {
        GD.Print();
        foreach (Move m in game.Pos.GenerateMoves())
        {
            GD.Print(m);
            if (m.ToString().Equals("O-O"))
            {
                GD.Print("Castling: ", m.FromSquare(),  "; ", m.ToSquare());
            }
        }
        GD.Print();
    }

    private static char PieceToChar(Piece piece)
    {
        return piece.ToString()[0];
    }

    private static void UpdateReadableState()
    {
        GameState state = new GameState();

        
        for (int y = 0; y < 8; y++)
        {
            String line = "";
            for (int x = 0; x < 8; x++)
            {
                Square sq = new Square(y, x);
                state.board[y,x] = PieceToChar(game.Pos.GetPiece(sq));
                line += state.board[y,x];
            }
            GD.Print(line);
        }

        state.WhiteCastlingRights = game.Pos.CanCastle(CastleRights.White);
        state.BlackCastlingRights = game.Pos.CanCastle(CastleRights.Black);

        GD.Print("Can white castle? " + state.WhiteCastlingRights);
        GD.Print("Can black castle? " + state.BlackCastlingRights);

        state.EnPassantSquare = game.Pos.EnPassantSquare.AsInt();
        if (game.Pos.InCheck)
        {
            if (game.Pos.SideToMove == Player.White)
            {
                state.WhiteChecked = true;
                state.BlackChecked = false;
            }
            else
            {
                state.WhiteChecked = false;
                state.BlackChecked = true;
            }
        }

        state.GameOver = game.Pos.GenerateMoves().Length > 0;
        state.WhiteToMove = (game.Pos.SideToMove == Player.White);
        state.HumanIsWhite = humanIsWhite;

        readableState = state;
    }

    private static void MakeMove(Move move)
    {
        game.Pos.MakeMove(move, game.Pos.State);
        UpdateReadableState();
    }
}
