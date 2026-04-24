using Godot;

public partial class Explosions : Node3D
{
    public override void _Ready()
    {
        foreach (Node child in GetChildren())
        {
            if (child is AnimatedSprite3D sprite)
                sprite.VisibilityChanged += () => OnSpriteVisible(sprite);
        }
    }

    private void OnSpriteVisible(AnimatedSprite3D sprite)
    {
        if (sprite.Visible)
            sprite.Play("default");
    }
}
