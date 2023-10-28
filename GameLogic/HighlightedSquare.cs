using Godot;
using System;
using System.ComponentModel;
using System.Linq;

public partial class HighlightedSquare : Sprite3D
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
                GD.Print(x + " " + y);
            }
        }

    }

    // Heavy function - avoid repeated calls
    private Vector2I FindChessPosition(Vector3 pos)
    {
        Marker3D closest = squareMarkers[0,0];
        float shortestDist = pos.DistanceTo(squareMarkers[0,0].Position);

        int x = 0;
        int y = 0;

        for (int i = 0; i < 8; i++)
        {
            for (int j = 0; j < 8; j++)
            {
                if (pos.DistanceTo(squareMarkers[i,j].Position) < shortestDist)
                {
                    closest = squareMarkers[i,j];
                    shortestDist = pos.DistanceTo(squareMarkers[i,j].Position);
                    x = j;
                    y = i;
                }
            }
        }

        return new Vector2I(x, y);
    }

    public override void _PhysicsProcess(double delta)
    {
        Vector3 raycastPosition = RaycastFromMouse(GetViewport().GetMousePosition());
        if (raycastPosition == Vector3.Zero)
        {
            Position = new Vector3(0, 100, 0);
        }
        else
        {
            Position = raycastPosition;
            ChessPosition = FindChessPosition(raycastPosition);

            float x = squareMarkers[ChessPosition.Y, ChessPosition.X].Position.X;
            float y = 0.112f;
            float z = squareMarkers[ChessPosition.Y, ChessPosition.X].Position.Z;

            Position = new Vector3(x, y, z);
        }
        
    }

    private Vector3 RaycastFromMouse(Vector2 mousePos)
    {
        Camera3D camera = GetNode<Camera3D>("/root/game_primary/Camera3D");

        
        Vector3 origin = camera.ProjectRayOrigin(mousePos);
        Vector3 direction = origin + camera.ProjectRayNormal(mousePos) * 25f;
        

        Godot.Collections.Dictionary result = GetWorld3D().DirectSpaceState.IntersectRay(PhysicsRayQueryParameters3D.Create(camera.Transform.Origin, direction));

        if (result.Count > 0)
        {
            return (Vector3)result["position"];
        }
                
        return Vector3.Zero;
    }
}
