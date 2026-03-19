using Godot;

public partial class InventoryUI : CanvasLayer
{
	[Export] private Player _player;
	[Export] private InventoryComponent _inventory;
	[Export] private VBoxContainer _itemsContainer;
	[Export] private PackedScene _itemUI;

	public override void _Ready()
	{
		_inventory.ItemAdded   += OnItemAdded;
		_inventory.ItemRemoved += OnItemRemoved;

		foreach (var item in _inventory.Items)
			OnItemAdded(item);

		Visible = false;
	}

	private void OnItemAdded(ItemInstance item)
	{
		var itemUI = _itemUI.Instantiate<ItemUI>();
		_itemsContainer.AddChild(itemUI);
		itemUI.Init(item);
	}

	private void OnItemRemoved(ItemInstance item)
	{
		foreach (var child in _itemsContainer.GetChildren())
		{
			if (child is ItemUI itemUI && itemUI.Item == item)
			{
				child.QueueFree();
				break;
			}
		}
	}
}
