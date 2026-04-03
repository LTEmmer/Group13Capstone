using Godot;

// Attach to any Node3D to make it spin continuously.
public partial class RotateComponent : Node3D
{
	[Export] public float DegreesPerSecond = 15.0f;
	[Export] public Vector3 Axis = Vector3.Up;

	public override void _Process(double delta)
	{
		RotateObjectLocal(Axis.Normalized(), Mathf.DegToRad(DegreesPerSecond) * (float)delta);
	}
}
