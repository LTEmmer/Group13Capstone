using Godot;
using System;

public partial class Player : CharacterBody3D
{
	[Export]
	public float MouseSensitivity { get; set; } = 0.002f;

	[Export]
	public float MaxPitchDegrees { get; set; } = 89f;
	
	private Node3D _cameraPivot;
	private float _pitch;
	
	private Vector3 _targetVelocity = Vector3.Zero;
	private int _speed { get; set; } = 10;
	private int _sprintSpeed = 2;
	private bool _acceptKeyboardInput = true;
	
	public override void _Ready()
	{
		_cameraPivot = GetNode<Node3D>("CameraPivot");
		Input.MouseMode = Input.MouseModeEnum.Captured;
		SceneManager.Instance.PreloadUI(SceneNames.PauseMenu_ButtonPanel);
		SceneManager.Instance.PreloadUI(SceneNames.PauseMenu_ViewFlashcards);
	}

	public override void _Input(InputEvent @event)
	{
		if (Input.MouseMode == Input.MouseModeEnum.Captured)
		{
			if (@event is InputEventMouseMotion motion)
			{
				RotateY(-motion.Relative.X * MouseSensitivity);
				_pitch -= motion.Relative.Y * MouseSensitivity;
				_pitch = Mathf.Clamp(_pitch, -Mathf.DegToRad(MaxPitchDegrees), Mathf.DegToRad(MaxPitchDegrees));
				_cameraPivot.Rotation = new Vector3(_pitch, 0, 0);
			}
		}
	}

	public override void _UnhandledInput(InputEvent @event)
	{
		// Only open the pause menu if no UI is currently open.
		// If UI is already open, let it handle ESC itself.
		if (@event.IsActionPressed("ui_cancel") && SceneManager.Instance.CurrentUI == null)
		{
			OpenPauseMenu();
		}
	}

	private void OpenPauseMenu()
	{
		SceneManager.Instance.SetUI(SceneNames.PauseMenu_ButtonPanel);
		Input.MouseMode = Input.MouseModeEnum.Visible;
	}

	public static void CaptureMouse()
	{
		Input.MouseMode = Input.MouseModeEnum.Captured;
	}

	public void SetAcceptKeyboardInput(bool accept)
	{
		_acceptKeyboardInput = accept;
	}

	public override void _PhysicsProcess(double delta)
	{
		if (_acceptKeyboardInput)
		{
			Vector2 inputDir = Input.GetVector("left", "right", "forward", "backward");
			Vector3 direction = (Transform.Basis * new Vector3(inputDir.X, 0, inputDir.Y)).Normalized();
			if (direction != Vector3.Zero)
			{
				Velocity = new Vector3(direction.X * _speed, Velocity.Y, direction.Z * _speed);
				if (Input.IsActionPressed("sprint"))
					Velocity = Velocity * _sprintSpeed;
				}
			else
			{
				Velocity = new Vector3(Mathf.MoveToward(Velocity.X, 0, _speed), Velocity.Y, Mathf.MoveToward(Velocity.Z, 0, _speed));
			}
		}

		MoveAndSlide();
	}
}