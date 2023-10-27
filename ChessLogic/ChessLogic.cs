using Godot;
using Rudzoft.ChessLib;
using Rudzoft.ChessLib.Factories;
using Rudzoft.ChessLib.MoveGeneration;
using Rudzoft.ChessLib.Types;
using System;
using System.Linq;
using System.Reflection.Metadata;

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
    }
}

public static class ChessLogic
{

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
    static ChessLogic()
    {
        readableState = new GameState();
        game = (Game)GameFactory.Create("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1");
        
        OpponentStupid playerOne = new OpponentStupid(true);
        OpponentStupid playerTwo = new OpponentStupid(false);                

        Move nextMove;

        for (int i = 0; i < 5; i++)
        {
            if (playerOne.IsTurn())
                nextMove = playerOne.GetMove(game.Pos);
            else
                nextMove = playerTwo.GetMove(game.Pos);

            MakeMove(nextMove);
            GD.Print(i + ": " + nextMove.FromSquare().ToString() + "-" + nextMove.ToSquare().ToString());
        }        
    }

    /**
    Returns a struct with all game information needed to visually represent the board in game.
    */
    public static GameState GetBoardState()
    {
        return readableState;
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
                state.board[y,x] = PieceToChar(game.Pos.GetPiece(new Square(y, x)));
                line += state.board[y,x];
            }
            GD.Print(line);
        }

        state.WhiteCastlingRights = game.Pos.CanCastle(CastleRights.White);
        state.BlackCastlingRights = game.Pos.CanCastle(CastleRights.Black);

        GD.Print(state.WhiteCastlingRights);
        GD.Print(state.BlackCastlingRights);
    }

    private static void MakeMove(Move move)
    {
        game.Pos.MakeMove(move, game.Pos.State);
        UpdateReadableState();
    }
}
