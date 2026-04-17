using Godot;

[GlobalClass]
public partial class ModAttack: ItemEffect
{
	// Delta Values
    [Export] public int _attack = 0;
	[Export] public float _crit_chance = 0;
	[Export] public float _crit_mult = 0;
	[Export] public float _attack_mult = 0;

    public override void Apply(Node target, ItemInstance item)
    {
		DoModAttack(target, true);
    }

    public override void Remove(Node target)
	{
		DoModAttack(target, false);
	}

	private void DoModAttack(Node target, bool apply)
	{
    	var attack = target.GetNodeOrNull<AttackComponent>("AttackComponent");
    	if (attack == null)
    	{
        	return;
    	}

    	if (apply)
    	{
        	attack.BaseMult += _attack_mult;
        	attack.BaseDamage += _attack;
        	attack.CritChance += _crit_chance;
        	attack.CritMult += _crit_mult;
    	}
    	else
    	{
        	attack.BaseMult -= _attack_mult;
        	attack.BaseDamage -= _attack;
        	attack.CritChance -= _crit_chance;
        	attack.CritMult -= _crit_mult;
    	}

    	attack.BaseDamage = Mathf.Max(attack.BaseDamage, 1);
    	attack.BaseMult   = Mathf.Max(attack.BaseMult, 0.1f);
    	attack.CritChance = Mathf.Max(attack.CritChance, 0f);
    	attack.CritMult   = Mathf.Max(attack.CritMult, 1f);
	}
}