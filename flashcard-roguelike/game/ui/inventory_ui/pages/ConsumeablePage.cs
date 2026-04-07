using Godot;

public partial class ConsumablePage : Control
{
    [Export] private Button _useButton;  // VBoxContainer/HBoxContainer2/Button
    [Export] private Button _dropButton; // VBoxContainer/HBoxContainer2/Button2

    private ItemInstance _item;

    public override void _Ready()
    {
        _useButton.Pressed  += OnUsePressed;
        _dropButton.Pressed += OnDropPressed;
    }

    public void SetItem(ItemInstance item)
    {
        _item = item;
    }

    private void OnUsePressed()
    {
        if (_item == null) return;
        GD.Print($"[Consumable] Use: {_item}");
    }

    private void OnDropPressed()
    {
        if (_item == null) return;
        GD.Print($"[Consumable] Drop: {_item}");
    }
}