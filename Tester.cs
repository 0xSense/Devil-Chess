using Godot;
using System;
using System.Diagnostics;

public partial class Tester : Marker2D
{
    public override void _Ready()
    {
        ChessLogic.AIMoveFinished += OnAIMove; // new ChessLogic.AIMoveNotify(OnAIMove);
        ChessLogic.NewGame(true);
        GD.Print(ChessLogic.SubmitHumanMove(5, 1, 5, 3));
    }

    public void OnAIMove()
    {
        GD.Print("Event detected");
    }
}
