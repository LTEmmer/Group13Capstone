using Godot;

public partial class ToolPage : InventoryPage<ItemResource>
{
    [Export] private Button _equipButton;
    [Export] private Button _dropButton;
	[Export] private RichTextLabel description;

    private new ItemResource _item;

    public override void _Ready()
    {
        _equipButton.Pressed += OnEquipPressed;
        _dropButton.Pressed  += OnDropPressed;
    }

    public override void SetItem(ItemResource item)
    {
        _item = item;
		description.Text = item.Description;
		OnItemSet(_item);

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