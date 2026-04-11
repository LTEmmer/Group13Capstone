using Godot;

[GlobalClass]
public partial class ModAttack: ItemEffect
{
	// Delta Values
    [Export] public int _attack = 0;
	[Export] public float _crit_chance = 0;
	[Export] public float _attack_mult = 1;

    public override void Apply(Node target, ItemResource item)
    {
		var attack = target.GetNodeOrNull<AttackComponent>("AttackComponent");
		if (attack== null)
		{
			GD.Print("ModAttack: Target has no attack component");
			return;
		}

		// TODO: either these methods need implementation, 
		// the attack dammage in the component needs to be able to be changed,
		// the attack ammout has some sort of additive value,
		// or some other way you can think of. :D
		// maby add a crit chance value in the attack component that can be changed
		// attack.setAttack();
		// attack.addAttack();
		// attack.multAttack();
    }
}