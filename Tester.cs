using Godot;
using System;
using System.Diagnostics;
using System.Threading;

public partial class Tester : Node3D
{
    public override void _Ready()
    {
        ChessLogic.AIMoveFinished += OnAIMove; // new ChessLogic.AIMoveNotify(OnAIMove);
        ChessLogic.NewGame(true);
        new Thread(PlayGame).Start();  
    }

    public void OnAIMove()
    {
        GD.Print("Event detected");
    }

    private void PlayGame()
    {
        GD.Print(ChessLogic.SubmitHumanMove(5, 1, 5, 3));
    }
}
