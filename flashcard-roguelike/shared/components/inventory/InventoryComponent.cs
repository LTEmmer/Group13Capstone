using Godot;
using Godot.Collections;

public partial class InventoryComponent : Node
{
	[Signal] public delegate void ItemAddedEventHandler(ItemInstance item);
	[Signal] public delegate void ItemRemovedEventHandler(ItemInstance item);

	[Export] public Array<ItemInstance> inv = new Array<ItemInstance>();

	public void AddItem(ItemInstance item)
	{
		AddItem(item.Resource, item.Count);
	}

	public void AddItem(ItemResource resource, int count = 1)
	{
		foreach (var item in inv)
		{
			if (item.Resource.Name == resource.Name)
			{
				item.Count += count;
				return;
			}
		}

		var newItem = new ItemInstance(resource, count);
		inv.Add(newItem);
		EmitSignal(SignalName.ItemAdded, newItem);
	}

	public void RemoveItem(ItemInstance item)
	{
		if (!inv.Remove(item)) return;
		EmitSignal(SignalName.ItemRemoved, item);
	}
}
