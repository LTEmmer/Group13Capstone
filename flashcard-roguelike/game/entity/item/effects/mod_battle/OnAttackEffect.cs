using Godot;
using Godot.Collections;

/// <summary>
/// Fires a list of ItemEffects on every successful attack.
/// Cannot contain another OnAttackEffect (enforced at Apply time).
/// </summary>
[GlobalClass]
public partial class OnAttackEffect : ItemEffect
{
    [Export] public Array<ItemEffect> OnAttackEffects { get; set; } = new();

    private Callable _callback;
    private AttackComponent _attackComp;
    private bool _connected = false;

    public override void Apply(Node target, ItemInstance item)
    {
        _attackComp = target.GetNodeOrNull<AttackComponent>("AttackComponent");
        if (_attackComp == null || _connected) return;

        _callback = Callable.From<Node, float>((hitTarget, damage) => OnAttack(target));
        _attackComp.Connect(AttackComponent.SignalName.OnAttackSuccessful, _callback);
        _connected = true;
    }

    public override void Remove(Node target)
    {
        if (!_connected || _attackComp == null) return;

        _attackComp.Disconnect(AttackComponent.SignalName.OnAttackSuccessful, _callback);
        _connected = false;
    }

    private void OnAttack(Node target)
    {
        foreach (var effect in OnAttackEffects)
        {
            if(effect is OnAttackEffect) continue;
            effect?.Apply(target, null);
        }
    }
}