using Godot;
using System;

public partial class Player : CharacterBody3D
{
	[Export]
	public CanvasLayer PauseMenu;

	[Export]
	public InventoryUI InventoryUI;

	[Export] 
	public Node3D CameraMount;
	[Export]
	// _speed of camera
	public float MouseSensitivity { get; set; } = 0.002f;

	[Export]
	// High/Low angle player can look (currently almost straight up/down)
	public float MaxPitchDegrees { get; set; } = 89f;
	
	//Input and PlayerModel
	[Export]
	public InputGatherer inputGatherer;
	[Export]
	public Model playerModel;
	
	
	// player components
	[Export]
	public AttackComponent attackComponent;
	[Export]
	public HealthComponent healthComponent;
	[Export]
	public InventoryComponent inventoryComponent;
	[Export]
	public StaminaComponent staminaComponent;
	
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
		// Ensure we have a reference to InventoryUI
		if (InventoryUI == null)
			InventoryUI = GetNodeOrNull<InventoryUI>("CameraPivot/Camera3D/InventoryUI");
	}

	public override void _Input(InputEvent @event)
	{
		if (InventoryUI != null)
		{
			if (@event.IsActionPressed("inventory_toggle"))
				InventoryUI.SetVisible(true);
			else if (@event.IsActionReleased("inventory_toggle"))
				InventoryUI.SetVisible(false);
		}

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
	public void toggleMouseLock()
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

	public void SetAcceptKeyboardInput(bool accept)
	{
		_acceptKeyboardInput = accept;
	}

public override void _PhysicsProcess(double delta)
	{
		//Get input 
		if (_acceptKeyboardInput){
			
			InputPackage input = inputGatherer.GatherInput();
			playerModel.Update(input,delta);
			input.QueueFree();
			
		} else {
			
			playerModel.SwitchTo(StateNames.idle);
			
		}
	}
}
