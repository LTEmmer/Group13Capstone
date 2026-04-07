using Godot;

public partial class EquipmentPage : Control
{
    [Export] private Button _equipButton;  // VBoxContainer/HBoxContainer2/Button
    [Export] private Button _dropButton;   // VBoxContainer/HBoxContainer2/Button2

    private ItemInstance _item;

    public override void _Ready()
    {
        _equipButton.Pressed += OnEquipPressed;
        _dropButton.Pressed  += OnDropPressed;
    }

    public void SetItem(ItemInstance item)
    {
        _item = item;
        // update any labels/icons here when ready
    }

    private void OnEquipPressed()
    {
        if (_item == null) return;
        GD.Print($"[Equipment] Equip/Unequip: {_item}");
    }

    private void OnDropPressed()
    {
        if (_item == null) return;
        GD.Print($"[Equipment] Drop: {_item}");
    }
}