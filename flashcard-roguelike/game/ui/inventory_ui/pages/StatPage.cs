using Godot;

public partial class StatPage : InventoryPage<ItemResource> 
{
    [Export] private Button _dropButton;
	[Export] private RichTextLabel description;

    private new ItemInstance _item;

    public override void _Ready()
    {
        _dropButton.Pressed += OnDropPressed;
    }

    public override void SetItem(ItemInstance item)
    {
        _item = item;
		description.Text = item.Resource.Description;
		OnItemSet(_item);

    }

    private void OnDropPressed()
    {
        if (_item == null) return;
        GD.Print($"[Stat] Drop: {_item}");
    }
}