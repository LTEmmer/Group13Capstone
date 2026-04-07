using Godot;

[GlobalClass]
public partial class ItemResource : Resource
{
    // ===== Basic Info =====
    [Export] public string Name = "";
    [Export] public Texture2D Icon;
    [Export] public Mesh Mesh;
    [Export(PropertyHint.MultilineText)] public string Description = "";

    // ===== Behavior =====
    public enum ItemBehavior { Stat, Consumeable, Equipable }
    [Export] public ItemBehavior Behavior;

    // ===== Rarity (1 = Common, 5 = Legendary) =====
    [Export(PropertyHint.Range, "1,5")] public int Rarity = 1;

    // ===== Uses =====
    [Export] public int MaxUses = -1; // -1 = infinite

    // ===== Effects =====
    [Export] public Godot.Collections.Array<ItemEffect> Effects = new();
}