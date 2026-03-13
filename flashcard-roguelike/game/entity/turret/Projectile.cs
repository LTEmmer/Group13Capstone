using Godot;
using System;

public partial class Projectile : Area3D
{
    [Export] public float Speed = 30f;
    [Export] public float MaxLifetime = 5f;
    private Vector3 _direction;
    private float _elapsed = 0f;

    public void Initialize(Vector3 direction) => _direction = direction.Normalized();

    public override void _Ready()
    {
        AreaEntered += OnAreaEntered;
    }

    public override void _PhysicsProcess(double delta)
    {
        _elapsed += (float)delta;
        if (_elapsed >= MaxLifetime) { QueueFree(); return; }
        GlobalPosition += _direction * Speed * (float)delta;
    }

    private void OnAreaEntered(Area3D area)
    {
        GD.Print("Projectile hit: " + area.Name + " of " + area.GetParent().Name);

        if (area is QAPanel panel)
        {
            panel.OnHit();
        }  
    }
}
