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
	
	[Export] public AudioStream[] FootstepSounds;
	[Export] public AudioStream[] JumpSounds;
	[Export] public AudioStream[] LandSounds;

	private Node3D _cameraPivot;
	private float _pitch;
	public bool SuppressNextLandSound { get; set; } = false;

	private Vector3 _targetVelocity = Vector3.Zero;
	private int _speed { get; set; } = 10;
	private int _sprintSpeed = 2;
	private bool _acceptKeyboardInput = true;

	private AudioStreamPlayer3D _footstepSoundPlayer;
	private AudioStreamPlayer3D _jumpSoundPlayer;
	private float _footstepTimer = 0f;
	private const float WalkStepInterval = 0.5f;
	private const float SprintStepInterval = 0.3f;
	private const float WalkPitch = 0.9f;
	private const float SprintPitch = 1.2f;
	private const float PitchVariance = 0.08f;
	
	public override void _Ready()
	{
		_cameraPivot = GetNode<Node3D>("CameraPivot");
		Input.MouseMode = Input.MouseModeEnum.Captured;
		// Ensure we have a reference to InventoryUI
		if (InventoryUI == null)
			InventoryUI = GetNodeOrNull<InventoryUI>("CameraPivot/Camera3D/InventoryUI");

		_footstepSoundPlayer = GetNode<AudioStreamPlayer3D>("FootstepSoundPlayer");
		_jumpSoundPlayer = GetNode<AudioStreamPlayer3D>("JumpSoundPlayer");
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
		else if (BattleManager.Instance == null || !BattleManager.Instance.IsInCombat)
		{
			// Only recapture the mouse if we're not in combat (combat keeps mouse visible for UI interaction)
			Input.MouseMode = Input.MouseModeEnum.Captured;
		}
	}

	public void SetAcceptKeyboardInput(bool accept)
	{
		_acceptKeyboardInput = accept;
	}

	public void ForceLookAt(Vector3 target)
	{	
		// Rotate the player to face the target horizontally
		LookAt(new Vector3(target.X, GlobalPosition.Y, target.Z), Vector3.Up);

		// Rotate the camera pivot to look at the target vertically, slightly lower
		_cameraPivot.LookAt(new(target.X, target.Y - 1, target.Z), Vector3.Up);
	}

public override void _PhysicsProcess(double delta)
	{
		//Get input 
		if (_acceptKeyboardInput){
			
			InputPackage input = inputGatherer.GatherInput();
			playerModel.Update(input,delta);
			TickFootsteps((float)delta);
			input.QueueFree();
		}else{
			playerModel.SwitchTo(StateNames.idle);
		}
	}

	public void PlayJumpSound()
	{
		if (JumpSounds == null || JumpSounds.Length == 0) return;

		_jumpSoundPlayer.Stream = JumpSounds[GD.Randi() % (uint)JumpSounds.Length];
		_jumpSoundPlayer.PitchScale = 1.0f + GD.Randf() * PitchVariance * 2f - PitchVariance;
		_jumpSoundPlayer.Play();
	}

	public void PlayLandSound()
	{
		if (SuppressNextLandSound) // For after connections
		{ 
			SuppressNextLandSound = false; 
			return; 
		}

		if (LandSounds == null || LandSounds.Length == 0) return;

		_jumpSoundPlayer.Stream = LandSounds[GD.Randi() % (uint)LandSounds.Length];
		_jumpSoundPlayer.PitchScale = 1.0f + GD.Randf() * PitchVariance * 2f - PitchVariance;
		_jumpSoundPlayer.Play();
	}

	private void TickFootsteps(float delta)
	{
		float speed = Velocity.Length();
		bool isSprinting = speed > 15f;

		if (!IsOnFloor() || speed <= 0.5f)
		{
			_footstepTimer = 0f;
			return;
		}

		_footstepTimer -= delta;
		if (_footstepTimer > 0f) return;

		if (FootstepSounds != null && FootstepSounds.Length > 0)
		{
			float basePitch = isSprinting ? SprintPitch : WalkPitch;
			_footstepSoundPlayer.Stream = FootstepSounds[GD.Randi() % (uint)FootstepSounds.Length];
			_footstepSoundPlayer.PitchScale = basePitch + GD.Randf() * PitchVariance * 2f - PitchVariance;
			_footstepSoundPlayer.Play();
		}

		_footstepTimer = isSprinting ? SprintStepInterval : WalkStepInterval;
	}
}
