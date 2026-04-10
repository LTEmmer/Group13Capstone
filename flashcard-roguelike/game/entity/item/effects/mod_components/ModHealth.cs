using Godot;

[GlobalClass]
public partial class ModHealth: ItemEffect
{
    [Export] public int _hp = 1;

    public override void Apply(Node target, ItemInstance item)
    {
        GD.Print($"Healing {_hp} to {target.Name}");
		var health = target.GetNodeOrNull<HealthComponent>("HealthComponent");
		if (health == null)
		{
			GD.Print("ModHealth: Target has no health component");
			return;
		}
		
		if(_hp == 0)return;
		if(_hp >= 0) health.Heal(_hp);
		else health.TakeDamage(_hp);
    }
}