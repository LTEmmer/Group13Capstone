using Godot;
using System;

[GlobalClass]
public partial class ItemInstance : GodotObject
{
    public event Action Changed;

    public ItemResource Resource { get; private set; }
    public int CurrentUses { get; set; }

    private int _count = 1;
    public int Count
    {
        get => _count;
        set
        {
            _count = value;
            Changed?.Invoke();
        }
    }

    public ItemInstance(ItemResource resource, int count = 1)
    {
        Resource = resource;
        CurrentUses = resource.MaxUses;
        _count = count;
    }
}