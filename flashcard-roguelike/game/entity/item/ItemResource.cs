using Godot;

[GlobalClass]
public partial class ItemResource : Resource
{
    // ===== Basic Info =====
    [Export] public string Name = "";
    [Export] public Texture2D Icon;
    [Export(PropertyHint.MultilineText)] public string Description = "";

    // ===== Behavior =====
    public enum ItemBehavior
    {
        Collectable,
        StatUpgrade
    }
    [Export] public ItemBehavior Behavior;

    [Export] public int MaxUses = -1;   // -1 = infinite
    public int CurrentUses = 0;

    // ===== Effects =====
    [Export] public Godot.Collections.Array<ItemEffect> Effects = new();
}