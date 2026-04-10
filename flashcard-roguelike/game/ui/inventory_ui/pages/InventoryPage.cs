using Godot;

// Generic base class for all inventory pages
public abstract partial class InventoryPage<T> : Control where T : ItemResource
{
    protected ItemInstance _item;

    /// <summary>
    /// Default method to set the item for this page
    /// </summary>
    public virtual void SetItem(ItemInstance item)
    {
        _item = item;
        OnItemSet(item);
    }

    /// <summary>
    /// Optional override for derived pages to react when an item is set
    /// </summary>
    protected virtual void OnItemSet(ItemInstance item)
    {
        GD.Print("OnItemSet: " + item.Resource.Name + " set.");
    }
}