using Godot;
using System;

[GlobalClass]
public partial class PickupEffect: ItemEffect
{
    [Export] public int Amount = 0;

    public override void Apply(Node target, ItemInstance itemInstance)
    {
		var inventory = target.GetNodeOrNull<InventoryComponent>("InventoryComponent");
		if (inventory == null) 
			throw new ArgumentNullException(nameof(target), "Target has no inventory!");

		ItemResource item = itemInstance.Resource;

		inventory.AddItem(item, Amount);
		GD.Print($"Added {Amount} '{item.Name}' to {target.Name}'s inventory.");
    }
}