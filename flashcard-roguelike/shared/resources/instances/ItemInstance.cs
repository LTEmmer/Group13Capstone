using Godot;
using System;

[GlobalClass]
[Tool]
public partial class ItemInstance : Resource
{
    [Signal] public delegate void ChangedEventHandler();

    // Export as generic Resource
    [Export] 
    public Resource ResourceBase { get; set; }

    // Safe typed accessor
    public ItemResource Resource
    {
        get => ResourceBase as ItemResource; // will be null if wrong type
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

    // Parameterless constructor for Godot
    public ItemInstance() { }
    public ItemInstance(ItemResource resource, int count = 1)
    {
        Resource = resource;
        CurrentUses = resource.MaxUses;
        Count = count;
        
    }
}