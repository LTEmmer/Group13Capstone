using Godot;
using System;

public partial class Tmp : Node3D
{
	[Export] Node3D inv;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
    	inv.CallDeferred(Node3D.MethodName.Show);
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
