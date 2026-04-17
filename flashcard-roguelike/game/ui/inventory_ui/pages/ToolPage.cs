using Godot;
using EquipType = ItemResource.EquipType;

public partial class ToolPage : InventoryPage<ItemResource>
{
    [Export] private Button _equipButton;
    [Export] private Button _dropButton;

    [Export] private TextureRect helm;
    [Export] private TextureRect chest;
    [Export] private TextureRect legs;
    [Export] private TextureRect left;
    [Export] private TextureRect right;
    [Export] private TextureRect charm1;
    [Export] private TextureRect charm2;
    [Export] private TextureRect charm3;
    [Export] private TextureRect charm4;

    [Export] private EquipmentComponent _equipment;

    public override void _Ready()
    {
        _equipButton.Pressed += OnEquipPressed;
        _dropButton.Pressed  += OnDropPressed;

        _equipment.ItemEquipped   += _ => Refresh();
        _equipment.ItemUnequipped += _ => Refresh();

        Refresh();
    }

    public override void SetItem(ItemInstance item)
    {
        base.SetItem(item);

        _equipButton.Text   = _equipment.IsEquipped(item) ? "Unequip" : "Equip";
        _dropButton.Visible = !_equipment.IsEquipped(item);
        _dropButton.Disabled = false;
        _equipButton.Disabled  = false;

        Refresh();
    }

    private void OnEquipPressed()
    {
        if (BattleManager.Instance.IsInCombat)
        {
           return; 
        }
        if (_item == null) return;

        if (_equipment.IsEquipped(_item))
        {
            _dropButton.Disabled = false;
            _equipment.Unequip(_item);
        }
        else
        {
            _dropButton.Disabled = true;
            _equipment.Equip(_item);
        }

        SetItem(_item);
    }

    private void OnDropPressed()
    {
        if (BattleManager.Instance.IsInCombat)
        {
           return; 
        }
        if (_item == null) return;
        _dropButton.Disabled = true;
        _equipButton.Disabled  = true;
        Drop(_item);
    }

    private void Refresh()
    {
        helm.Texture   = GetIcon(EquipType.Helmet);
        chest.Texture  = GetIcon(EquipType.Chestplate);
        legs.Texture   = GetIcon(EquipType.Leggings);
        left.Texture   = GetIcon(EquipType.LeftHand);
        right.Texture  = GetIcon(EquipType.RightHand);
        charm1.Texture = GetIcon(EquipType.Charm1);
        charm2.Texture = GetIcon(EquipType.Charm2);
        charm3.Texture = GetIcon(EquipType.Charm3);
        charm4.Texture = GetIcon(EquipType.Charm4);
    }

    private Texture2D GetIcon(EquipType slot)
        => _equipment.GetEquipped(slot)?.Resource?.Icon;
}