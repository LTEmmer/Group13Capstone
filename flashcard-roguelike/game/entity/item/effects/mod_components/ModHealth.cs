using Godot;

[GlobalClass]
public partial class ModHealth: ItemEffect
{
    [Export] public int _hp = 0;
	[Export] private int _maxHp = 0;
	[Export] private float _trueDefence;

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
		int hp = apply? _hp : -_hp;
		float trueDefence = apply? _trueDefence: -_trueDefence;

		GD.Print("ModHP: ", hp);
		var health = target.GetNodeOrNull<HealthComponent>("HealthComponent");
		if (health == null)
		{
			GD.PrintErr("ModHealth: Target has no health component");
			return;
		}
		
		health.MaxHealth += _maxHp;
		if(hp != 0)
		{
			if(hp > 0) health.Heal(hp);
			else health.TakeDamage(-hp, true);
		}

		GD.Print("TD: ", trueDefence);
		health.TrueDefence += trueDefence;
    	health.TrueDefence = Mathf.Min(health.TrueDefence, 2f);
		GD.Print("TDA: ", health.TrueDefence);
    	health.MaxHealth = Mathf.Max(health.MaxHealth, 1);
	}

}