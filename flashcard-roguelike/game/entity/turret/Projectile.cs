using Godot;
using System;

public partial class Projectile : Area3D
{
    [Export] public float Speed = 30f;
    [Export] public float MaxLifetime = 5f;
    [Export] public AudioStream[] ImpactSounds;
    [Export] public MeshInstance3D Visual;

    private static readonly Color[] PrimaryColors =
    {
        new Color(1, 0, 0),   // Red
        new Color(0, 1, 0),   // Green
        new Color(0, 0, 1),   // Blue
        new Color(1, 1, 0),   // Yellow
        new Color(0, 1, 1),   // Cyan
        new Color(1, 0, 1),   // Magenta
    };

    private Vector3 _direction;
    private float _elapsed = 0f;
    private AudioStreamPlayer3D _audioPlayer;
    private Color _color;
    private bool _hasHit = false;

    public void Initialize(Vector3 direction)
    {
        _direction = direction.Normalized();
        _color = PrimaryColors[GD.Randi() % PrimaryColors.Length];
    }

    public override void _Ready()
    {
        AreaEntered += OnAreaEntered;
        BodyEntered += OnBodyEntered;
        _audioPlayer = GetNode<AudioStreamPlayer3D>("AudioPlayer");

        var mat = new StandardMaterial3D();
        mat.AlbedoColor = _color;
        mat.EmissionEnabled = true;
        mat.Emission = _color * 2f; // Make it glow
        Visual.SetSurfaceOverrideMaterial(0, mat);
    }

    public override void _PhysicsProcess(double delta)
    {
        _elapsed += (float)delta;
        if (_elapsed >= MaxLifetime) { QueueFree(); return; }
        GlobalPosition += _direction * Speed * (float)delta;
    }

    private void OnAreaEntered(Area3D area)
    {
        if (_hasHit) return;
        _hasHit = true;

        GD.Print("Projectile hit area: " + area.Name);
        PlayImpactSound();

        if (area is QAPanel panel)
            panel.OnHit();

        QueueFree();
    }

    private void OnBodyEntered(Node3D body)
    {
        if (_hasHit) return;
        _hasHit = true;

        GD.Print("Projectile hit body: " + body.Name);
        PlayImpactSound();
        QueueFree();
    }

    private void PlayImpactSound()
    {
        if (ImpactSounds == null || ImpactSounds.Length == 0) return;
        _audioPlayer.Stream = ImpactSounds[GD.Randi() % ImpactSounds.Length];
        _audioPlayer.Reparent(GetTree().Root);
        _audioPlayer.Play();
        _audioPlayer.Finished += _audioPlayer.QueueFree;
    }
}
