using Godot;
using System;

public partial class Player : Node3D
{
	[Export]
	// Speed of camera
	public float MouseSensitivity { get; set; } = 0.002f;

	[Export]
	// High/Low angle player can look (currently almost straight up/down)
	public float MaxPitchDegrees { get; set; } = 89f;

	private Node3D _cameraPivot;
	private float _pitch;

	public override void _Ready()
	{
		_cameraPivot = GetNode<Node3D>("CameraPivot");
		Input.MouseMode = Input.MouseModeEnum.Captured;
	}

	public override void _Input(InputEvent @event)
	{
		if (@event is InputEventMouseMotion motion)
		{
			// Horizontal: rotate the whole player (yaw)
			RotateY(-motion.Relative.X * MouseSensitivity);

			// Vertical: rotate only the camera (pitch)
			_pitch -= motion.Relative.Y * MouseSensitivity;
			_pitch = Mathf.Clamp(_pitch, -Mathf.DegToRad(MaxPitchDegrees), Mathf.DegToRad(MaxPitchDegrees));
			_cameraPivot.Rotation = new Vector3(_pitch, 0, 0);
		}
	}
}
