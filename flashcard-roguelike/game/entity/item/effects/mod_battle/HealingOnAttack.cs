using Godot;

[GlobalClass]
public partial class HealingOnAttack : ItemEffect
{
    [Export] public int HealAmount = 5;  // Amount to heal per successful attack
    private Callable _callback;
    private AttackComponent _attackComp;
    private bool _connected = false;

    public override void Apply(Node target, ItemInstance item)
    {
        _attackComp = target.GetNode<AttackComponent>("AttackComponent");
        if (_attackComp != null && !_connected)
        {
            _callback = Callable.From<Node, float>((t, d) => Heal(target));
            _attackComp.Connect(AttackComponent.SignalName.OnAttackSuccessful, _callback);
            _connected = true;
        }
    }

    public override void Remove(Node target)
    {
        if (_connected && _attackComp != null)
        {
            _attackComp.Disconnect(AttackComponent.SignalName.OnAttackSuccessful, _callback);
            _connected = false;
        }
    }

    private void Heal(Node target)
    {
        var health = target.GetNode<HealthComponent>("HealthComponent");
        if (health != null)
        {
            health.Heal(HealAmount);
            GD.Print($"{target.Name} healed for {HealAmount} HP from attack!");
        }
    }
}