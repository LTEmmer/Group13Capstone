using Godot;
using System;

public partial class Turret : Node3D
{
    [Signal]
    public delegate void OutOfAmmoEventHandler();

    [Export] public PackedScene ProjectileScene;
    [Export] public float TurretSensitivity = 0.002f;
    [Export] public float PitchLimitDegrees = 80f;
    [Export] public float ZoomFov = 30f;
    [Export] public AudioStream[] ShootSounds;
    [Export] public MeshInstance3D PaintMesh;

    private Node3D _turretBase;
    private Node3D _turretHead;
    private Camera3D _turretCamera;
    private int _ammo;
    private Marker3D _barrelTip;
    private float _defaultFov;
    private float _currentPitch = 0f;
    private bool _isActive = false;
    private AudioStreamPlayer3D _audioPlayer;

    public override void _Ready()
    {
        _turretBase = GetNode<Node3D>("TurretBase");
        _turretHead = _turretBase.GetNode<Node3D>("TurretHead");
        _turretCamera = _turretHead.GetNode<Camera3D>("TurretView");
        _barrelTip = _turretHead.GetNode<Marker3D>("BarrelTip");
        _audioPlayer = _turretHead.GetNode<AudioStreamPlayer3D>("AudioPlayer");

        _defaultFov = _turretCamera.Fov;
    }

    public void SetAmmo(int ammo)
    {
        _ammo = ammo;
    }

    public int GetAmmo()
    {
        return _ammo;
    }

    public void Fire(Vector3 direction)
    {
        if (_ammo <= 0) // failsafe
        {
            EmitSignal(nameof(OutOfAmmo));
            return;
        }

        // Play shoot sound
        if (ShootSounds != null && ShootSounds.Length > 0)
        {
            _audioPlayer.Stream = ShootSounds[GD.Randi() % ShootSounds.Length];
            _audioPlayer.PitchScale = (float)GD.RandRange(.75, 1.25); // Slight random pitch variation
            _audioPlayer.Play();
        }

        Projectile projectile = ProjectileScene.Instantiate<Projectile>();
        projectile.Initialize(direction);
        GetParent().AddChild(projectile);
        projectile.GlobalPosition = _barrelTip.GlobalPosition;

        _ammo--;

        // Signal when out of ammo
        if (_ammo <= 0)
        {
            EmitSignal(nameof(OutOfAmmo));
        }
    }

    public void ActivateTurret(Camera3D playerCam)
    {
        _isActive = true;
        playerCam.Current = false; // Deactivate player camera
        _turretCamera.Current = true; // Enable turret camera
    }

    public void DeactivateTurret(Camera3D playerCam)
    {
        _isActive = false;
        _turretCamera.Current = false; // Disable turret camera and remove it
        _turretCamera.QueueFree();
        playerCam.Current = true; // Reactivate player camera
        _turretCamera.Fov = _defaultFov; // Reset FOV
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (!_isActive) return;

        if (@event is InputEventMouseMotion motion)
        {
            // Yaw: rotate the base horizontally
            _turretBase.RotateY(-motion.Relative.X * TurretSensitivity);

            // Pitch: rotate the head vertically around local Z (barrel is along local -X, so Z is the pitch axis)
            _currentPitch -= motion.Relative.Y * TurretSensitivity;
            _currentPitch = Mathf.Clamp(_currentPitch, Mathf.DegToRad(-8), Mathf.DegToRad(PitchLimitDegrees));
            _turretHead.Rotation = new Vector3(0, 0, -_currentPitch);
        }

        if (@event is InputEventMouseButton mouse)
        {
            if (mouse.ButtonIndex == MouseButton.Left && mouse.Pressed)
            {
                // Fire in the barrel's forward direction (-X in local space)
                Vector3 direction = -_turretHead.GlobalTransform.Basis.X;
                Fire(direction);
            }

            if (mouse.ButtonIndex == MouseButton.Right)
            {
                // Toggle zoom
                _turretCamera.Fov = mouse.Pressed ? ZoomFov : _defaultFov;
            }
        }
    }
}
