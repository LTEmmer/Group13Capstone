using Godot;

public partial class UsePage : InventoryPage<ItemResource>
{
    [Export] private Button _useButton;
    [Export] private Button _dropButton;
	[Export] private RichTextLabel description;

    private new ItemInstance _item;

    public override void _Ready()
    {
        _useButton.Pressed  += OnUsePressed;
        _dropButton.Pressed += OnDropPressed;
    }

    public override void SetItem(ItemInstance item)
    {
        _item = item;
		description.Text = item.Resource.Description;
		OnItemSet(_item);

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