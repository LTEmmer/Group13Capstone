using Godot;
using System.Collections.Generic;

public partial class InventoryComponent : Node
{
    [Signal] public delegate void ItemAddedEventHandler(ItemInstance item);
    [Signal] public delegate void ItemRemovedEventHandler(ItemInstance item);

    private readonly List<ItemInstance> _items = new();
    public IReadOnlyList<ItemInstance> Items => _items;

    public void AddItem(ItemResource resource, int count = 1)
    {
        foreach (var item in _items)
        {
            if (item.Resource.Name == resource.Name)
            {
                item.Count += count; // triggers ItemInstance.Changed
                return;
            }
        }

        var newItem = new ItemInstance(resource, count);
        _items.Add(newItem);
        EmitSignal(SignalName.ItemAdded, newItem);
    }

    public void RemoveItem(ItemInstance item)
    {
        if (!_items.Remove(item)) return;
        EmitSignal(SignalName.ItemRemoved, item);
    }
}