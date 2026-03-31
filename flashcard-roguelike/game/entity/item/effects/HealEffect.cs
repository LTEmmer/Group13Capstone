using Godot;

[GlobalClass]
public partial class HealEffect : ItemEffect
{
    [Export] public int Amount = 0;

    public override void Apply(Node target, ItemInstance item)
    {
        // Example: call your player/health system here
        GD.Print($"Healing {Amount} to {target.Name}");
		var health = target.GetNodeOrNull<HealthComponent>("HealthComponent");
        if (health != null) health.Heal(Amount);
    }
}