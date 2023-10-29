using Godot;
using System;

public partial class GameManager : Node
{
    public override void _Ready()
    {
        GetNode<LoadSignaller>("/root/game_primary").SceneLoaded += OnLoad;
    }

    public void OnLoad()
    {
        GetNode<HighlightedSquare>("/root/game_primary/HighlightedSquare").SquareClicked += HandleClick;
        ChessLogic.AIMoveFinished += OnAIMove;
        ChessLogic.NewGame(true);
    }

    public void HandleClick(Vector2I square)
    {
        GD.Print(ToAlgebraicCoords(square));
    }

    public void OnAIMove()
    {

    }

    private Vector2I ToAlgebraicCoords(Vector2I v)
    {
        return new Vector2I(v.X, 7-v.Y);
    }
}
