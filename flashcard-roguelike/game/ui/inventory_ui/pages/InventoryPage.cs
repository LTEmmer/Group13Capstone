using Godot;

// Generic base class for all inventory pages
public abstract partial class InventoryPage<T> : Control where T : ItemResource
{
    protected ItemResource _item;

    /// <summary>
    /// Default method to set the item for this page
    /// </summary>
    public virtual void SetItem(ItemResource item)
    {
        _item = item;
        OnItemSet(item);
    }

    /// <summary>
    /// Optional override for derived pages to react when an item is set
    /// </summary>
    protected virtual void OnItemSet(ItemResource item)
    {
        GD.Print("OnItemSet: " + item.Name + " set.");
    }
}