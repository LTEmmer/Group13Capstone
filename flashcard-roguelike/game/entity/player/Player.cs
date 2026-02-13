using Godot;
using System;

public partial class Player : CharacterBody3D
{
	[Export]
	// _speed of camera
	public float MouseSensitivity { get; set; } = 0.002f;

	[Export]
	// High/Low angle player can look (currently almost straight up/down)
	public float MaxPitchDegrees { get; set; } = 89f;
	
	private Node3D _cameraPivot;
	private float _pitch;
	
	private Vector3 _targetVelocity = Vector3.Zero;
	private int _speed { get; set; } = 10;
	private int _sprintSpeed = 2;
	
	public override void _Ready()
	{
		_cameraPivot = GetNode<Node3D>("CameraPivot");
		Input.MouseMode = Input.MouseModeEnum.Captured;
	}

	public override void _Input(InputEvent @event)
	{
		if (Input.MouseMode == Input.MouseModeEnum.Captured)
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
	public override void _UnhandledInput(InputEvent @event)
	{
    	if (@event.IsActionPressed("ui_cancel"))
    	{
			if (Input.MouseMode == Input.MouseModeEnum.Captured)
			{
    			Input.MouseMode = Input.MouseModeEnum.Visible;
			}
			else
			{
    			Input.MouseMode = Input.MouseModeEnum.Captured;
			}
    	}
	}


public override void _PhysicsProcess(double delta)
	{
		Vector2 inputDir = Input.GetVector("left", "right", "forward", "backward");
		Vector3 direction = (Transform.Basis * new Vector3(inputDir.X, 0, inputDir.Y)).Normalized();
		if (direction != Vector3.Zero)
		{
			Velocity = new Vector3(direction.X * _speed, Velocity.Y, direction.Z * _speed);
			if (Input.IsActionPressed("sprint")){
				Velocity = Velocity * _sprintSpeed;
			}
		}
		else
		{
			Velocity = new Vector3(Mathf.MoveToward(Velocity.X, 0, _speed), Velocity.Y, Mathf.MoveToward(Velocity.Z, 0, _speed));
		}
		MoveAndSlide();
	}
}
