using Godot;
using Godot.Collections;

public partial class InventoryComponent : Node
{
    [Signal] public delegate void ItemAddedEventHandler(ItemInstance item);
    [Signal] public delegate void ItemRemovedEventHandler(ItemInstance item);

    [Export] public Array<ItemInstance> inv = new();
    [Export] public Player EffectTarget;

    public Array<ItemInstance> StatItems
    {
        get
        {
            var arr = new Array<ItemInstance>();
            foreach (var item in inv)
                if (item.Resource.Behavior == ItemResource.ItemBehavior.Stat)
                    arr.Add(item);
            return arr;
        }
    }

    public Array<ItemInstance> UseItems
    {
        get
        {
            var arr = new Array<ItemInstance>();
            foreach (var item in inv)
                if (item.Resource.Behavior == ItemResource.ItemBehavior.Use)
                    arr.Add(item);
            return arr;
        }
    }

    public Array<ItemInstance> ToolItems
    {
        get
        {
            var arr = new Array<ItemInstance>();
            foreach (var item in inv)
                if (item.Resource.Behavior == ItemResource.ItemBehavior.Tool)
                    arr.Add(item);
            return arr;
        }
    }

    // ===== Add Item =====
    public void AddItem(ItemResource resource) => AddItem(new ItemInstance(resource));

    public void AddItem(ItemInstance item)
    {

        GD.Print(item.PickupEffectsApplied);
        if (item.Resource.AddToInventory) inv.Add(item);
        if (!item.PickupEffectsApplied && item.Resource.PickupEffects != null && EffectTarget != null)
        {
            foreach (var effect in item.Resource.PickupEffects)
                effect.Apply(EffectTarget, item);
            item.PickupEffectsApplied = true;
        }
        EmitSignal(SignalName.ItemAdded, item);
    }

    // ===== Remove Item =====
    public void RemoveItem(ItemInstance item)
    {
        if (!inv.Remove(item)) return;
        if (item.PickupEffectsApplied && item.Resource.PickupEffects != null && EffectTarget != null)
        {
            foreach (var effect in item.Resource.PickupEffects)
                effect.Remove(EffectTarget);
            item.PickupEffectsApplied = false;
        }
        EmitSignal(SignalName.ItemRemoved, item);
    }
}