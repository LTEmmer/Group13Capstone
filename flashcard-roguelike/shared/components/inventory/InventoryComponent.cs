using Godot;
using System;
using System.Collections.Generic;

public partial class InventoryComponent : Node
{
	public partial class InventoryItem : GodotObject
	{
		public string Name { get; set; } = string.Empty;
		public string Description { get; set; } = string.Empty;
		public int Count { get; set; } = 1;
	}

	[Signal]
	public delegate void InventoryChangedEventHandler();

	private readonly List<InventoryItem> _items = new List<InventoryItem>();

	public IReadOnlyList<InventoryItem> Items => _items;

	public void AddItem(string name, string description)
	{
		if (string.IsNullOrWhiteSpace(name) && string.IsNullOrWhiteSpace(description))
		{
			return;
		}

		// Simple stacking: same name + description increments count
		foreach (var item in _items)
		{
			if (item.Name == name && item.Description == description)
			{
				item.Count++;
				EmitSignal(SignalName.InventoryChanged);
				return;
			}
		}

		var newItem = new InventoryItem
		{
			Name = name ?? string.Empty,
			Description = description ?? string.Empty,
			Count = 1
		};

		_items.Add(newItem);
		EmitSignal(SignalName.InventoryChanged);
	}
}

