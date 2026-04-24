using Godot;
using Godot.Collections;

public partial class InventoryUI : Node3D
{
	public enum Pages { Stat, Use, Tool }

	[Export] private InventoryComponent _inventory;
	[Export] private Player _player;
	[Export] private EquipmentComponent _equipment;
	[Export] private Color _equipHighlightColor = new Color(1f, 0.85f, 0.2f, 0.35f);

	[Export] private PackedScene _itemScene;

	[Export] private StatPage _statPage;
	[Export] private ToolPage _toolPage;
	[Export] private UsePage _usePage;

	[Export] private Control _statItems;
	[Export] private Control _toolItems;
	[Export] private Control _useItems;

	[Export] private ItemList _statList;
	[Export] private ItemList _useList;
	[Export] private ItemList _toolList;

	[Export] private Button _statTab;
	[Export] private Button _useTab;
	[Export] private Button _toolTab;

	public override void _Ready()
	{
		ProcessMode = ProcessModeEnum.Always;

		_inventory.ItemAdded += OnItemAdded;
		_inventory.ItemRemoved += OnItemRemoved;

		_statList.ItemSelected += OnItemSelected;
		_useList.ItemSelected += OnItemSelected;
		_toolList.ItemSelected += OnItemSelected;

		_usePage.DropRequested += HandleDrop;
		_statPage.DropRequested += HandleDrop;
		_toolPage.DropRequested += HandleDrop;

		_usePage.UseRequested += HandleUse;

		_statTab.Pressed += () => ShowPage(_statPage, _statItems);
		_useTab.Pressed  += () => ShowPage(_usePage,  _useItems);
		_toolTab.Pressed += () => ShowPage(_toolPage, _toolItems);

		if (_equipment != null)
		{
			_equipment.ItemEquipped   += _ => RefreshEquipHighlights();
			_equipment.ItemUnequipped += _ => RefreshEquipHighlights();
			_equipment.EffectTarget = _player;
		}

		RefreshUILists();

		ShowPage(_toolPage, _toolItems);
		Visible = true;
	}

	// ───────────────────────── UI SYNCHRONIZATION ─────────────────────────

	private void RefreshUILists()
	{
		_statList.Clear();
		_useList.Clear();
		_toolList.Clear();

		foreach (var item in _inventory.StatItems)
			_statList.AddItem("", item.Resource.Icon);
		foreach (var item in _inventory.UseItems)
			_useList.AddItem("", item.Resource.Icon);
		foreach (var item in _inventory.ToolItems)
			_toolList.AddItem("", item.Resource.Icon);
	}

	// ───────────────────────── ITEM MANAGEMENT ─────────────────────────

	private void OnItemAdded(ItemInstance item)
	{
		if (item?.Resource == null)
		{
			GD.PrintErr("OnItemAdded: null item");
			return;
		}

		var res = item.Resource;

		// PickupEffects are applied in Item.Interact, not here

		if (res.Behavior == ItemResource.ItemBehavior.Stat
			&& res.UseEffects != null)
		{
			foreach (var effect in res.UseEffects)
				effect.Apply(_player, item);
		}

		RefreshUILists();
		RefreshEquipHighlights();
	}

	private void OnItemRemoved(ItemInstance item)
	{
		// Stat items: remove passive UseEffects when leaving inventory
		if ( item.Resource.Behavior == ItemResource.ItemBehavior.Stat
			&& item.Resource.UseEffects != null)
		{
			foreach (var effect in item.Resource.UseEffects)
				effect.Remove(_player);
		}

		RefreshUILists();
		RefreshEquipHighlights();
	}

	// ───────────────────────── SELECTION ─────────────────────────

	private void OnItemSelected(long index)
	{
		var (arr, page) = GetOpenPage();

		if (index < 0 || index >= arr.Count)
			return;

		var item = arr[(int)index];

		GD.Print($"[Inventory] Selected: {item.Resource.Name}");

		page.SetItem(item);
	}

	// ───────────────────────── ACTIONS ─────────────────────────

	private void HandleDrop(ItemInstance item)
	{
		if (item == null) return;

		GD.Print($"[Inventory] Drop: {item.Resource.Name}");

		_inventory.RemoveItem(item);

		SpawnItem(item);
	}

	private void HandleUse(ItemInstance item)
	{
		if (item == null) return;

		GD.Print($"[Inventory] Use: {item.Resource.Name}");

		if (item.Resource.UseEffects != null)
		{
			foreach (ItemEffect effect in item.Resource.UseEffects)
				effect.Apply(_player, item);
		}

		if (item.Resource.MaxUses > 0)
		{
			item.CurrentUses--;

			if (item.CurrentUses <= 0)
			{
				_inventory.RemoveItem(item);
				return;
			}
		}

		var (_, page) = GetOpenPage();
		page.SetItem(item);
	}

	// ───────────────────────── SPAWN ─────────────────────────

	private void SpawnItem(ItemInstance item)
	{
		if (_player == null || _itemScene == null)
		{
			GD.PrintErr("[Inventory] Missing player or item scene");
			return;
		}

		var newItem = _itemScene.Instantiate<Item>();

		_player.GetParent().AddChild(newItem);
		item.PickupEffectsApplied = true;

		newItem.Init(item);

		newItem.GlobalPosition = _player.GlobalPosition - new Vector3(0, 0.5f, 0);
	}

	// ───────────────────────── EQUIP HIGHLIGHT ─────────────────────────

	private void RefreshEquipHighlights()
	{
		if (_equipment == null) return;

		ApplyHighlights(_statList, _inventory.StatItems);
		ApplyHighlights(_useList,  _inventory.UseItems);
		ApplyHighlights(_toolList, _inventory.ToolItems);
	}

	private void ApplyHighlights(ItemList list, Array<ItemInstance> arr)
	{
		for (int i = 0; i < arr.Count; i++)
		{
			bool equipped = _equipment.IsEquipped(arr[i]);
			list.SetItemCustomBgColor(i, equipped ? _equipHighlightColor : Colors.Transparent);
		}
	}

	// ───────────────────────── UI HELPERS ─────────────────────────

	private (Array<ItemInstance> arr, InventoryPage<ItemResource> page) GetOpenPage()
	{
		if (_statPage.Visible) return (_inventory.StatItems, _statPage);
		if (_usePage.Visible) return (_inventory.UseItems, _usePage);
		return (_inventory.ToolItems, _toolPage);
	}

	private void ShowPage(Control page, Control items)
	{
        AudioManager.Instance.PlayButtonClick();
		
		_toolPage.Visible = false;
		_toolItems.Visible = false;
		_statPage.Visible = false;
		_statItems.Visible = false;
		_usePage.Visible = false;
		_useItems.Visible = false;

		page.Visible = true;
		items.Visible = true;
	}
}
