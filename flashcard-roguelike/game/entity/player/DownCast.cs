using Godot;
using System;

public partial class DownCast : RayCast3D
{
	[Export] 
	public ModifierBoneTarget3D HipsAttachment;
	[Export] 
	public CsgSphere3D RootSphere;
	[Export]
	public CsgSphere3D TargetSphere;

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		GlobalPosition = HipsAttachment.GlobalPosition;
		RootSphere.GlobalPosition = GlobalPosition;
		TargetSphere.GlobalPosition = GetCollisionPoint();
	}
}
