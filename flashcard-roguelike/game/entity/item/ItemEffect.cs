using Godot;

[GlobalClass]
public abstract partial class ItemEffect : Resource
{
    public abstract void Apply(Node target, ItemInstance item);

    public virtual void Remove(Node target) { }
}