using Godot;
using System;
using System.ComponentModel;

public partial class InventoryUI : Control 
{

	private InventoryComponent _inventory;
	private Player _player;
	private VBoxContainer _itemsContainer;

	public override void _Ready()
	{
		_itemsContainer = GetNodeOrNull<VBoxContainer>("Panel/MarginContainer/VBoxContainer/ScrollContainer/ItemsContainer");
		Visible = false;
	}

	public void SetPlayer(Player player)
	{
		_player = player;
		_inventory = _player.GetNodeOrNull<InventoryComponent>("InventoryComponent");
	}

	// //>Show or hide the inventory. Used for hold-TAB-to-view.
	// public void SetVisible(bool visible)
	// {
	// 	Visible = visible;
	// 	if (visible)
	// 	{
	// 		EnsureInventory();
	// 		RefreshInventoryList();
	// 	}
	// }

	private void OnInventoryChanged()
	{
		if (Visible)
		{
			RefreshInventoryList();
		}
	}

	private void RefreshInventoryList()
	{
		if (_itemsContainer == null || _inventory == null)
			return;

		foreach (Node child in _itemsContainer.GetChildren())
		{
			_itemsContainer.RemoveChild(child);
			child.QueueFree();
		}

		foreach (var item in _inventory.Items)
		{
			var row = new HBoxContainer();

			var nameLabel = new Label();
			string countSuffix = item.Count > 1 ? $" x{item.Count}" : string.Empty;
			nameLabel.Text = $"{item.Name}{countSuffix}";
			nameLabel.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
			nameLabel.AddThemeFontSizeOverride("font_size", 18);
			row.AddChild(nameLabel);

			var descriptionLabel = new Label();
			descriptionLabel.Text = item.Description;
			descriptionLabel.AutowrapMode = TextServer.AutowrapMode.WordSmart;
			descriptionLabel.CustomMinimumSize = new Vector2(300, 0);
			row.AddChild(descriptionLabel);

			_itemsContainer.AddChild(row);
		}
	}
}

