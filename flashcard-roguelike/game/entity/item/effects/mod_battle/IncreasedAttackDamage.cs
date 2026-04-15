using Godot;

[GlobalClass]
public partial class IncreasedAttackDamage : ItemEffect
{
    [Export] public float DamageBonus = 10f;

    public override void Apply(Node target, ItemInstance item)
    {
        var attack = target.GetNode<AttackComponent>("AttackComponent");
        if (attack != null)
        {
            attack.BaseDamage += DamageBonus;
            GD.Print($"{target.Name}'s attack damage increased by {DamageBonus}!");
        }
    }

    public override void Remove(Node target)
    {
        var attack = target.GetNode<AttackComponent>("AttackComponent");
        if (attack != null)
        {
            attack.BaseDamage -= DamageBonus;
        }
    }
}