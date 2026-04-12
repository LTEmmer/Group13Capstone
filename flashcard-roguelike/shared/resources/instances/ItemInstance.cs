using Godot;

[GlobalClass]
[Tool]
public partial class ItemInstance : Resource
{
    [Signal] public delegate void ChangedEventHandler();

    [Export]
    public Resource ResourceBase { get; set; }

    public ItemResource Resource
    {
        get => ResourceBase as ItemResource;
        set => ResourceBase = value;
    }

    [Export] public int CurrentUses { get; set; }

    private int _count = 1;
    [Export]
    public int Count
    {
        get => _count;
        set
        {
            if (_count != value)
            {
                _count = value;
                EmitSignal(nameof(Changed));
            }
        }
    }

    // The slot array and index this item occupies, null if not equipped
    public ItemInstance[] EquippedSlot { get; set; }
    public int EquippedIndex { get; set; }
    public bool IsEquipped => EquippedSlot != null;

    public ItemInstance() { }
    public ItemInstance(ItemResource resource, int count = 1)
    {
        Resource = resource;
        CurrentUses = resource.MaxUses;
        Count = count;
    }
}