using System;
using Godot;

/// <summary>
/// Generic base class for all inventory pages.
/// Pages fire events; InventoryUI owns the actual logic.
/// </summary>
public abstract partial class InventoryPage<T> : Control where T : ItemResource
{
    protected ItemInstance _item;
    [Export] private RichTextLabel _description;
    [Export] private Label _name;

    // ── Events ──────────────────────────────────────────────────────────────
    public event Action<ItemInstance> DropRequested;
    public event Action<ItemInstance> UseRequested;
    public event Action<ItemInstance> EquipRequested;

    // ── Public API ──────────────────────────────────────────────────────────
    public virtual void SetItem(ItemInstance item)
    {
        OnItemSet(item);
    }

    // ── Protected helpers ───────────────────────────────────────────────────
    protected virtual void OnItemSet(ItemInstance item)
    {
        _item = item;
        _description.Text = item.Resource.Description;
        _name.Text = item.Resource.Name;
    }

    protected void Drop(ItemInstance item)
    {
        if (item == null)
        {
            GD.PrintErr("RequestDrop called with null item");
            return;
        }

        DropRequested?.Invoke(item);
    }
    protected void Use(ItemInstance item)
    {
        if (item == null)
        {
            GD.PrintErr("RequestDrop called with null item");
            return;
        }

        UseRequested?.Invoke(item);
    }
    protected void Equip(ItemInstance item)
    {
        if (item == null)
        {
            GD.PrintErr("RequestDrop called with null item");
            return;
        }

        EquipRequested?.Invoke(item);
    }
}