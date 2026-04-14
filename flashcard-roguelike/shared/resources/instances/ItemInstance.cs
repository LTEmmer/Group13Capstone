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

    private bool _pickupEffectsApplied = false;

    [Export]
    public bool PickupEffectsApplied
    {
        get => _pickupEffectsApplied;
        set => _pickupEffectsApplied = value;
    }

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

    // Set by EquipmentComponent when equipped; null when in the bag.
    public ItemResource.EquipType? ActiveSlot { get; set; }
    public bool IsEquipped => ActiveSlot.HasValue;

    public ItemInstance() { }
    public ItemInstance(ItemResource resource, int count = 1)
    {
        Resource    = resource;
        CurrentUses = resource.MaxUses;
        Count       = count;
        PickupEffectsApplied = false;
        GD.Print($"ItemInstance created for {resource?.Name}, PickupEffectsApplied = {PickupEffectsApplied}");
    }

    public ItemInstance Clone() => new ItemInstance
    {
        Resource               = Resource,
        CurrentUses            = CurrentUses,
        Count                  = Count,
        ActiveSlot             = ActiveSlot,
        PickupEffectsApplied   = PickupEffectsApplied
    };
}