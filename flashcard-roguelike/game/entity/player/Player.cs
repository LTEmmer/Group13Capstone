using Godot;
using System;
using System.Text.RegularExpressions;

public partial class Player : CharacterBody3D
{
	[Export]
	public CanvasLayer PauseMenu;

	[Export]
	public Inventory InventoryCanvas;

	[Export]
	public Node3D CameraMount;

	[Export] 
	public RayCast3D sightline;

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

	public Camera3D PlayerCamera;

	private Node3D _cameraPivot;
	private float _pitch;
	public bool SuppressNextLandSound { get; set; } = false;

	private Vector3 _targetVelocity = Vector3.Zero;
	private int _speed { get; set; } = 10;
	private int _sprintSpeed = 2;
	private bool _acceptKeyboardInput = true;
	private const float PitchVariance = 0.1f; // Random pitch variance for footstep sounds

	public AudioStreamPlayer3D FootstepSoundPlayer;
	public AudioStreamPlayer3D JumpSoundPlayer;

	private Interactable oldInteractable = null;

    public override void _EnterTree()
    {
        base._EnterTree();
		AddToGroup("player");
    }

	public override void _Ready()
	{
		_cameraPivot = GetNode<Node3D>("CameraPivot");
		PlayerCamera = _cameraPivot.GetNode<Camera3D>("Camera3D");
		Input.MouseMode = Input.MouseModeEnum.Captured;

		FootstepSoundPlayer = GetNode<AudioStreamPlayer3D>("FootstepSoundPlayer");
		JumpSoundPlayer = GetNode<AudioStreamPlayer3D>("JumpSoundPlayer");
	}
	
	public override void _Input(InputEvent @event)
	{
		if (@event.IsActionPressed("inventory_toggle") && _acceptKeyboardInput)
		{
			InventoryCanvas.SetVisible(true);
			toggleMouseLock();
		}
		else if (@event.IsActionReleased("inventory_toggle") && InventoryCanvas.Visible)
		{
			InventoryCanvas.SetVisible(false);
			toggleMouseLock();
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

		if (@event.IsActionPressed("interact") && sightline.IsColliding())
		{

			var colider = sightline.GetCollider() as Node;
			if (colider.IsInGroup("Interactable"))
			{
        		if (colider is Interactable interactable){
            		interactable.Interact(this);
        		}
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

	// ref is a keyword that modifies the reference
	public void TickFootsteps(ref float footstepTimer, float delta, float stepInterval, float basePitch)
	{
		if (!IsOnFloor() || Velocity.Length() <= 0.5f)
		{
			footstepTimer = 0f;
			return;
		}

		footstepTimer -= delta;
		
		if (footstepTimer > 0f) 
		{
			return;
		}

		if (FootstepSounds != null && FootstepSounds.Length > 0)
		{
			FootstepSoundPlayer.Stream = FootstepSounds[GD.Randi() % (uint)FootstepSounds.Length];
			FootstepSoundPlayer.PitchScale = basePitch + (float)GD.RandRange(-PitchVariance, PitchVariance);
			FootstepSoundPlayer.Play();
		}

		footstepTimer = stepInterval;
	}

	public override void _PhysicsProcess(double delta)
	{
		Interactable newInteractable = null;

    	if (sightline.IsColliding())
    	{
        	var collider = sightline.GetCollider() as Node;
        	if (collider is Interactable interactable)
            	newInteractable = interactable;
    	}

    	if (newInteractable != oldInteractable)
    	{
        	if (IsInstanceValid(oldInteractable))
            	oldInteractable?.HoverEnd(this);
        	if (IsInstanceValid(newInteractable))
            	newInteractable?.HoverStart(this);

        	oldInteractable = newInteractable;
    	}

		//Get input
		if (_acceptKeyboardInput){

			InputPackage input = inputGatherer.GatherInput();
			playerModel.Update(input,delta);
			input.QueueFree();
		}else{
			playerModel.SwitchTo(StateNames.idle);
		}
	}
}
