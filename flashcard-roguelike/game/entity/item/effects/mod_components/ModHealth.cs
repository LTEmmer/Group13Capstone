using Godot;

[GlobalClass]
public partial class ModHealth: ItemEffect
{
    [Export] public int _hp = 1;
	[Export] private bool _maxHp;

    public override void Apply(Node target, ItemInstance item)
	{
		DoModHealth(target, true);
	}

    public override void Remove(Node target)
    {
        base.Remove(target);
		DoModHealth(target, false);

    }

	private void DoModHealth(Node target, bool apply)
	{
		int hp = apply? _hp: -_hp;
        GD.Print($"Healing {hp} to {target.Name}");
		var health = target.GetNodeOrNull<HealthComponent>("HealthComponent");
		if (health == null)
		{
			GD.Print("ModHealth: Target has no health component");
			return;
		}
		
		if(hp == 0)return;
		else if (_maxHp)
		{
			health.MaxHealth += hp;
		}
		else if(hp >= 0) health.Heal(hp);
		else health.TakeDamage(-hp);

	}

}