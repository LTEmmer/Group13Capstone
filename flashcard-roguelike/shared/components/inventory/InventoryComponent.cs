using Godot;
using Godot.Collections;

public partial class InventoryComponent : Node
{
    [Signal] public delegate void ItemAddedEventHandler(ItemInstance item);
    [Signal] public delegate void ItemRemovedEventHandler(ItemInstance item);

    [Export] public Array<ItemInstance> inv = new();

    // ===== Add Item =====
    public void AddItem(ItemResource resource) => AddItem(new ItemInstance(resource));

    public void AddItem(ItemInstance item)
    {
        inv.Add(item);
        EmitSignal(SignalName.ItemAdded, item);
    }

    // ===== Remove Item =====
    public void RemoveItem(ItemInstance item)
    {
        if (!inv.Remove(item)) return;
        EmitSignal(SignalName.ItemRemoved, item);
    }
}