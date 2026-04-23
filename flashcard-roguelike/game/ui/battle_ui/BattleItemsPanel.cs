using Godot;
using Godot.Collections;
using System;
using System.Collections.Generic;

public partial class BattleItemsPanel : Control
{
	public event Action OnBack;
	public event Action<ItemInstance> OnItemUsed;

	[Export] private PackedScene _slotScene;
	[Export] private GridContainer _itemGrid;
	[Export] private Button _backButton;
	[Export] private Button _useButton;

	private Array<ItemInstance> _items;
	private int _selectedIndex = -1;
	private List<BattleItemSlot> _slots = new();
	private Theme _tooltipTheme;

	public override void _Ready()
	{
		AudioManager.Instance?.RegisterButton(_backButton);
		AudioManager.Instance?.RegisterButton(_useButton);

		_backButton.Pressed += () => OnBack?.Invoke();
		_useButton.Pressed += OnUsePressed;
		_useButton.Disabled = true;
		Visible = false;

		_tooltipTheme = new Theme();
		_tooltipTheme.SetFontSize("font_size", "TooltipLabel", 18);
		_tooltipTheme.SetColor("background_color", "TooltipLabel", new Color(0f, 0f, 0f, 1f));
	}

	public void Populate(Array<ItemInstance> items)
	{
		_items = items;
		_selectedIndex = -1;
		_slots.Clear();
		_useButton.Disabled = true;

		foreach (Node child in _itemGrid.GetChildren())
		{
			child.QueueFree();
		}

		for (int i = 0; i < items.Count; i++)
		{
			var slot = _slotScene.Instantiate<BattleItemSlot>();
			_itemGrid.AddChild(slot);
			slot.Init(items[i], i, _tooltipTheme, OnItemSlotClicked);
			_slots.Add(slot);
		}
	}

	private void OnItemSlotClicked(int idx)
	{
		_selectedIndex = idx;
		_useButton.Disabled = false;
		for (int i = 0; i < _slots.Count; i++)
		{
			_slots[i].SetSelected(i == idx);
		}
	}

	private void OnUsePressed()
	{
		if (_selectedIndex < 0 || _items == null || _selectedIndex >= _items.Count) 
		{
			return;
		}
		OnItemUsed?.Invoke(_items[_selectedIndex]);
	}
}
