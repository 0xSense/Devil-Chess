using Godot;
using System;

public partial class Tester : Marker2D
{
    public override void _Ready()
    {
        GameState state = ChessLogic.GetBoardState();
    }
}
