using Godot;

[GlobalClass]
public partial class HurtEffect : ItemEffect
{
    [Export] public int Amount = 0;

    public override void Apply(Node target, ItemInstance item)
    {
        GD.Print($"Damaging {Amount} to {target.Name}");
		var health = target.GetNodeOrNull<HealthComponent>("HealthComponent");
        if (health != null) health.TakeDamage(Amount);
    }
}