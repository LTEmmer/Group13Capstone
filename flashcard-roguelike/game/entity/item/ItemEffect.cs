using Godot;

[GlobalClass]
public abstract partial class ItemEffect : Resource
{
    public virtual void Apply(Node target, ItemInstance item)
    {
        GD.Print("ItemEffect: Unimplemented Method");
    }

    public virtual void Remove(Node target){}
}