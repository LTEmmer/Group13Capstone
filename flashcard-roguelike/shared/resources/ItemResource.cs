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
    public enum ItemBehavior { Stat, Use, Tool }
    [Export] public ItemBehavior Behavior;

    // ===== Equipment =====
    // SlotGroup is what you set in the editor describes the *kind* of slot.
    // EquipType is the specific physical slot used internally by EquipmentComponent.
    public enum SlotGroup { None, Helmet, Chestplate, Leggings, Handheld, Charm }
    public enum EquipType  { 
        None, 
        Helmet, Chestplate, Leggings, 
        LeftHand, RightHand, 
        Charm1, Charm2, Charm3, Charm4 
    }

    [Export] public SlotGroup Slot = SlotGroup.None;

    // ===== Rarity (1 = Common, 5 = Legendary) =====
    [Export(PropertyHint.Range, "1,5")] public int Rarity = 1;

    [Export] public bool AddToInventory = true;
    [Export] public bool OnlyCombat = false;

    // ===== Uses =====
    [Export] public int MaxUses = -1; // -1 = infinite

    // ===== Effects =====
    [Export] public Array<ItemEffect> UseEffects;
    [Export] public Array<ItemEffect> PickupEffects;
}