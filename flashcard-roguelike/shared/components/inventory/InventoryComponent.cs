using Godot;
using Godot.Collections;

public partial class InventoryComponent : Node
{
	[Signal] public delegate void ItemAddedEventHandler(ItemResource item);
	[Signal] public delegate void ItemRemovedEventHandler(ItemResource item);

	[Export] public Array<ItemResource> inv = new Array<ItemResource>();

	public void AddItem(ItemResource resource)
	{
		inv.Add(resource);
		EmitSignal(SignalName.ItemAdded, resource);
	}

	public void RemoveItem(ItemResource item)
	{
		if (!inv.Remove(item)) return;
		EmitSignal(SignalName.ItemRemoved, item);
	}
}
