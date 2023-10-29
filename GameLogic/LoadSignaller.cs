using Godot;
using System;

public partial class LoadSignaller : Node3D
{
	public delegate void Start();
	public event Start SceneLoaded;

	public override void _Ready()
	{
		GD.Print("Signalling start");
		SceneLoaded?.Invoke();
	}
}
