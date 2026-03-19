using Godot;

[GlobalClass]
public partial class StatEffect : ItemEffect
{
    public enum StatTypeEnum { Strength, Health, Speed }

    [Export] public StatTypeEnum StatType;
    [Export] public int Amount;

    public override void Apply(Node target, ItemInstance item)
    {
        GD.Print($"Applying {Amount} {StatType} to {target.Name}");
    }

    public override void Remove(Node target)
    {
        GD.Print($"Removing {Amount} {StatType} from {target.Name}");
    }
}