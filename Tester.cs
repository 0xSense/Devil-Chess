using Godot;
using System;
using System.Diagnostics;
using System.Threading;

public partial class Tester : Node3D
{
    public override void _Ready()
    {
        ChessLogic.AIMoveFinished += OnAIMove; // new ChessLogic.AIMoveNotify(OnAIMove);
        ChessLogic.NewGame(true, "rnq1kbnr/1ppbppp1/p2p3p/8/3NPB2/3P4/PPP2PPP/RN1QKB1R w KQkq - 3 6");
        //ChessLogic.NewGame(true);
        new Thread(PlayGame).Start();  
    }

    public void OnAIMove()
    {
        GD.Print("Event detected");
    }

    private void PlayGame()
    {
        GD.Print(ChessLogic.SubmitHumanMove(7, 1, 7, 2));
    }
}
