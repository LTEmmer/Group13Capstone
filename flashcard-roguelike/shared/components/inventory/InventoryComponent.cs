using Godot;
using Godot.Collections;
using Slot = ItemResource.EquipSlot;

public partial class InventoryComponent : Node
{
	[Signal] public delegate void ItemAddedEventHandler(ItemInstance item);
	[Signal] public delegate void ItemRemovedEventHandler(ItemInstance item);
	[Signal] public delegate void ItemEquippedEventHandler(ItemInstance item);
	[Signal] public delegate void ItemUnequippedEventHandler(ItemInstance item);

	// ===== Inventory =====
	[Export] public Array<ItemInstance> inv = new();

	// ===== Equipment =====
	public class EquipmentSlots
	{
		public readonly System.Collections.Generic.Dictionary<Slot, ItemInstance[]> Slots = new()
		{
			{ Slot.Helmet,     new ItemInstance[1] },
			{ Slot.Chestplate, new ItemInstance[1] },
			{ Slot.Leggings,   new ItemInstance[1] },
			{ Slot.Handheld,   new ItemInstance[2] },
			{ Slot.Charm,      new ItemInstance[4] },
		};

		public ref ItemInstance Helmet     => ref Slots[Slot.Helmet][0];
		public ref ItemInstance Chestplate => ref Slots[Slot.Chestplate][0];
		public ref ItemInstance Leggings   => ref Slots[Slot.Leggings][0];
		public ref ItemInstance LeftHand   => ref Slots[Slot.Handheld][0];
		public ref ItemInstance RightHand  => ref Slots[Slot.Handheld][1];
		public ref ItemInstance Charm1     => ref Slots[Slot.Charm][0];
		public ref ItemInstance Charm2     => ref Slots[Slot.Charm][1];
		public ref ItemInstance Charm3     => ref Slots[Slot.Charm][2];
		public ref ItemInstance Charm4     => ref Slots[Slot.Charm][3];
	}

	public EquipmentSlots Equipped = new();

	// ===== Add Item =====
	public void AddItem(ItemResource item)
	{
		AddItem(new ItemInstance(item));
	}

	public void AddItem(ItemInstance item)
	{
		inv.Add(item);
		item.EquippedSlot = null;

		EmitSignal(SignalName.ItemAdded, item);
	}

	// ===== Remove Item =====
	public void RemoveItem(ItemInstance item)
	{
		if (!inv.Remove(item)) return;

		EmitSignal(SignalName.ItemRemoved, item);
	}

	// ===== Equip =====
	public void Equip(ItemInstance item)
	{
		if (item.Resource == null || item.Resource.Behavior != ItemResource.ItemBehavior.Tool)
			return;

		if (!Equipped.Slots.TryGetValue(item.Resource.Slot, out var slots))
			return;

		for (int i = 0; i < slots.Length; i++)
		{
			if (slots[i] != null) continue;

			slots[i] = item;
			item.EquippedSlot = slots;
			item.EquippedIndex = i;
			EmitSignal(SignalName.ItemEquipped, item);
			return;
		}
	}

	// ===== Unequip =====
	public void Unequip(ItemInstance item)
	{
		if (!item.IsEquipped) return;

		item.EquippedSlot[item.EquippedIndex] = null;
		item.EquippedSlot = null;
		EmitSignal(SignalName.ItemUnequipped, item);
	}
}