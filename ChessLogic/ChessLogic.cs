using Godot;
using Rudzoft.ChessLib;
using Rudzoft.ChessLib.Factories;
using Rudzoft.ChessLib.MoveGeneration;
using Rudzoft.ChessLib.Types;
using System;
using System.Linq;

public class ChessLogic
{
    private Game game;
    public ChessLogic()
    {
        game = (Game)GameFactory.Create("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1");
        
        OpponentStupid opponent = new OpponentStupid();
        Move move = opponent.GetMove(game.Pos);
        GD.Print("One");
        GD.Print(move.FromSquare().ToString() + "-" + move.ToSquare().ToString());
    }
}
