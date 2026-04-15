using Godot;
using System;

public partial class Painting : Node3D
{
    [Export] public Texture2D[] Paintings;

    public override void _Ready()
    {
        var sprite = GetNode<Sprite3D>("Sprite3D");
        sprite.Texture = Paintings[GD.Randi() % Paintings.Length];
    }
}
