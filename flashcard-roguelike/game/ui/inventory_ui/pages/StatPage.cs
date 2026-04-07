using Godot;

public partial class StatPage : Control
{
    [Export] private Button _dropButton; // VBoxContainer/HBoxContainer2/Button

    private ItemInstance _item;

    public override void _Ready()
    {
        _dropButton.Pressed += OnDropPressed;
    }

    public void SetItem(ItemInstance item)
    {
        _item = item;
    }

    private void OnDropPressed()
    {
        if (_item == null) return;
        GD.Print($"[Stat] Drop: {_item}");
    }
}