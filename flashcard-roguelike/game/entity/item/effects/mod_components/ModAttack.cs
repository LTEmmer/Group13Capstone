using Godot;

[GlobalClass]
public partial class ModAttack: ItemEffect
{
	// Delta Values
    [Export] public int _attack = 0;
	[Export] public float _crit_chance = 0;
	[Export] public float _attack_mult = 1;

    public override void Apply(Node target, ItemInstance item)
    {
		var attack = target.GetNodeOrNull<AttackComponent>("AttackComponent");
		if (attack== null)
		{
			GD.Print("ModAttack: Target has no attack component");
			return;
		}

		attack.BaseDamage += _attack;
		attack.BaseDamage *= _attack_mult;
		// Crit chance not implemented yet
    }

    public override void Remove(Node target)
    {
        var attack = target.GetNodeOrNull<AttackComponent>("AttackComponent");
		if (attack== null)
		{
			return;
		}

		attack.BaseDamage /= _attack_mult;
		attack.BaseDamage -= _attack;
    }
}