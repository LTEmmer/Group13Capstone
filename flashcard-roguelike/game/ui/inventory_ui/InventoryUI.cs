using Godot;
using System;

public partial class InventoryUI : CanvasLayer
{
	[Export] public NodePath InventoryComponentPath;

	private InventoryComponent _inventory;
	private VBoxContainer _itemsContainer;

	public override void _Ready()
	{
		_itemsContainer = GetNodeOrNull<VBoxContainer>("Panel/MarginContainer/VBoxContainer/ScrollContainer/ItemsContainer");

		if (!InventoryComponentPath.IsEmpty)
		{
			_inventory = GetNodeOrNull<InventoryComponent>(InventoryComponentPath);
		}
		else
		{
			// Try to locate the player and its InventoryComponent similar to HUD
			Node parent = GetParent();
			if (parent != null)
				parent = parent.GetParent();
			if (parent != null)
				parent = parent.GetParent();

			if (parent is Player player)
			{
				_inventory = player.GetNodeOrNull<InventoryComponent>("InventoryComponent");
			}
		}

		if (_inventory != null)
		{
			_inventory.Connect(InventoryComponent.SignalName.InventoryChanged, Callable.From(OnInventoryChanged));
			RefreshInventoryList();
		}

		Visible = false;
	}

	//Resolve the player's InventoryComponent (in case it wasn't ready at _Ready).
	private void EnsureInventory()
	{
		if (_inventory != null) return;
		Node parent = GetParent();
		if (parent != null) parent = parent.GetParent();
		if (parent != null) parent = parent.GetParent();
		if (parent is Player player)
			_inventory = player.GetNodeOrNull<InventoryComponent>("InventoryComponent");
	}

	//>Show or hide the inventory. Used for hold-TAB-to-view.
	public void SetVisible(bool visible)
	{
		Visible = visible;
		if (visible)
		{
			EnsureInventory();
			RefreshInventoryList();
		}
	}

	private void OnInventoryChanged()
	{
		if (Visible)
		{
			RefreshInventoryList();
		}
	}

	private void RefreshInventoryList()
	{
		EnsureInventory();
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
