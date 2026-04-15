using Godot;
using System.Collections.Generic;
using SG = ItemResource.SlotGroup;
using ET = ItemResource.EquipType;

public partial class EquipmentComponent : Node
{
    [Signal] public delegate void ItemEquippedEventHandler(ItemInstance item);
    [Signal] public delegate void ItemUnequippedEventHandler(ItemInstance item);

    [Export] public Player EffectTarget;

    // Maps each SlotGroup to the physical slots that accept it.
    private static readonly Dictionary<SG, ET[]> GroupSlots = new()
    {
        { SG.Helmet,     new[] { ET.Helmet } },
        { SG.Chestplate, new[] { ET.Chestplate } },
        { SG.Leggings,   new[] { ET.Leggings } },
        { SG.Handheld,   new[] { ET.LeftHand, ET.RightHand } },
        { SG.Charm,      new[] { ET.Charm1, ET.Charm2, ET.Charm3, ET.Charm4 } },
    };

    private readonly Dictionary<ET, ItemInstance> _slots = new();

    // ===== Equip =====
    public bool Equip(ItemInstance item)
    {
        if (item?.Resource == null) return false;

        var group = item.Resource.Slot;
        if (group == SG.None)
        {
            GD.PrintErr($"[Equipment] {item.Resource.Name} has no slot group.");
            return false;
        }

        if (!GroupSlots.TryGetValue(group, out var candidates))
            return false;

        foreach (var slot in candidates)
        {
            if (_slots.ContainsKey(slot)) continue;

            _slots[slot] = item;
            item.ActiveSlot = slot;
            GD.Print($"[Equipment] Equipped: {item.Resource.Name} → {slot}");

            // Tool items: apply UseEffects while equipped
            if (EffectTarget != null
                && item.Resource.Behavior == ItemResource.ItemBehavior.Tool
                && item.Resource.UseEffects != null)
            {
                foreach (var effect in item.Resource.UseEffects)
                    effect.Apply(EffectTarget, item);
            }

            EmitSignal(SignalName.ItemEquipped, item);
            return true;
        }

        GD.Print($"[Equipment] No free slot for {item.Resource.Name} (group: {group}).");
        return false;
    }

    // ===== Unequip =====
    public bool Unequip(ItemInstance item)
    {
        if (item?.Resource == null || !item.IsEquipped) return false;

        var slot = item.ActiveSlot!.Value;
        if (!_slots.TryGetValue(slot, out var current) || current != item)
            return false;

        _slots.Remove(slot);
        item.ActiveSlot = null;
        GD.Print($"[Equipment] Unequipped: {item.Resource.Name}");

        // Tool items: remove UseEffects when unequipped
        if (EffectTarget != null
            && item.Resource.Behavior == ItemResource.ItemBehavior.Tool
            && item.Resource.UseEffects != null)
        {
            foreach (var effect in item.Resource.UseEffects)
                effect.Remove(EffectTarget);
        }

        EmitSignal(SignalName.ItemUnequipped, item);
        return true;
    }

    // ===== Queries =====
    public ItemInstance GetEquipped(ET slot)
        => _slots.TryGetValue(slot, out var item) ? item : null;

    public bool IsSlotOccupied(ET slot)
        => _slots.ContainsKey(slot);

    public bool IsEquipped(ItemInstance item)
        => item?.IsEquipped == true
           && _slots.TryGetValue(item.ActiveSlot!.Value, out var current)
           && current == item;
}