using Godot;
using System;

public partial class RollableSprite : RigidBody3D
{
    [Export] public Texture2D SpriteTexture;

    public override void _Ready()
    {
        Sprite3D sprite = GetNodeOrNull<Sprite3D>("Sprite");
        sprite.Texture = SpriteTexture;
    }
}
