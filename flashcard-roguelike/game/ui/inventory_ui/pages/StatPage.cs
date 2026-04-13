using Godot;

public partial class StatPage : InventoryPage<ItemResource>
{
    [Export] private Button _dropButton;

    private new ItemInstance _item;

    public override void _Ready()
    {
        _dropButton.Pressed += OnDropPressed;
    }

    private void OnDropPressed()
    {
        if (_item == null) return;
        GD.Print("dropping");
        Drop(_item);
    }
}