using Godot;
using System;
using System.Runtime.CompilerServices;

public partial class PieceMovement : Sprite3D
{
	public Vector2I ChessPosition; // Chess coords, 0-indexed
	private static Marker3D[,] squareMarkers = new Marker3D[8,8];
	[Export] Node3D SquareMarkers;

	public override void _Ready()
	{
		ChessPosition = Vector2I.Zero;
		GetNode<LoadSignaller>("/root/game_primary").SceneLoaded += OnLoad;       
	}
	private void OnLoad()
	{
		if (squareMarkers[0,0] == null)
		{
			for (int i = 0; i < 64; i++)
			{
				int y, x;

				x = i%8;
				y = i/8;

				squareMarkers[y,x] = SquareMarkers.GetChild<Marker3D>(i);
			}
		}

		ChessPosition = FindChessPosition();
	}

	// Heavy function - avoid repeated calls
	private Vector2I FindChessPosition()
	{
		Marker3D closest = squareMarkers[0,0];
		float shortestDist = Position.DistanceTo(squareMarkers[0,0].Position);

		int x = 0;
		int y = 0;

		for (int i = 0; i < 8; i++)
		{
			for (int j = 0; j < 8; j++)
			{
				if (Position.DistanceTo(squareMarkers[i,j].Position) < shortestDist)
				{
					closest = squareMarkers[i,j];
					shortestDist = Position.DistanceTo(squareMarkers[i,j].Position);
					x = j;
					y = i;
				}
			}
		}

		return new Vector2I(x, y);
	}

	public override void _Process(double delta)
	{
		float x = squareMarkers[ChessPosition.Y, ChessPosition.X].Position.X;
		float y = 0.19f;
		float z = squareMarkers[ChessPosition.Y, ChessPosition.X].Position.Z;

		Position = new Vector3(x, y, z);
		//GD.Print(Position);
	}

    public void MoveToChessPosition(Vector2I coords)
    {
        GD.Print("Moving to: " + coords);
        ChessPosition = coords;
        Marker3D closest = squareMarkers[0,0];
        float shortestDist = Position.DistanceTo(squareMarkers[0,0].Position);

        for (int i = 0; i < 8; i++)
        {
            for (int j = 0; j < 8; j++)
            {
                if (Position.DistanceTo(squareMarkers[i,j].Position) < shortestDist)
                {
                    closest = squareMarkers[i,j];
                    shortestDist = Position.DistanceTo(squareMarkers[i,j].Position);                    
                }
            }
        }
        
        Position = closest.Position;
    }


}
