using Godot;
using Godot.Collections;

[GlobalClass]
public partial class ItemResource : Resource
{
    // ===== Basic Info =====
    [Export] public string Name = "";
    [Export] public Texture2D Icon;
    [Export] public Mesh Mesh;
    [Export(PropertyHint.MultilineText)] public string Description = "";

    // ===== Behavior =====
    public enum ItemBehavior { Stat, Use , Tool}
    [Export] public ItemBehavior Behavior;

    // ===== Equipment =====
    public enum EquipSlot
    {
        None,
        Helmet,
        Chestplate,
        Leggings,
        Handheld,
        Charm,
    }

    [Export] public EquipSlot Slot = EquipSlot.None;

    // ===== Rarity (1 = Common, 5 = Legendary) =====
    [Export(PropertyHint.Range, "1,5")] public int Rarity = 1;

    [Export] public bool AddToInventory = true;
    // ===== Uses =====
    [Export] public int MaxUses = -1; // -1 = infinite

    // ===== Effects =====
    [Export] public Array<ItemEffect> UseEffects;
    [Export] public Array<ItemEffect> PickupEffects;
}