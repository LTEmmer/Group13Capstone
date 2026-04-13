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

    private Array<ItemInstance> _statArr = new();
    private Array<ItemInstance> _useArr = new();
    private Array<ItemInstance> _toolArr = new();

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

        // NEW — refresh highlights whenever equipment changes
        if (_equipment != null)
        {
            _equipment.ItemEquipped   += _ => RefreshEquipHighlights();
            _equipment.ItemUnequipped += _ => RefreshEquipHighlights();
        }

        foreach (ItemInstance item in _inventory.inv)
            OnItemAdded(item);

        ShowPage(_toolPage, _toolItems);
        Visible = true;
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

        switch (res.Behavior)
        {
            case ItemResource.ItemBehavior.Stat:
                _statList.AddItem("", res.Icon);
                _statArr.Add(item);
                break;

            case ItemResource.ItemBehavior.Use:
                _useList.AddItem("", res.Icon);
                _useArr.Add(item);
                break;

            case ItemResource.ItemBehavior.Tool:
                _toolList.AddItem("", res.Icon);
                _toolArr.Add(item);
                break;
        }

        RefreshEquipHighlights(); // NEW — newly added item might already be equipped
    }

    private void OnItemRemoved(ItemInstance item)
    {
        int idx;

        switch (item.Resource.Behavior)
        {
            case ItemResource.ItemBehavior.Stat:
                idx = _statArr.IndexOf(item);
                if (idx < 0) return;
                _statList.RemoveItem(idx);
                _statArr.RemoveAt(idx);
                break;

            case ItemResource.ItemBehavior.Use:
                idx = _useArr.IndexOf(item);
                if (idx < 0) return;
                _useList.RemoveItem(idx);
                _useArr.RemoveAt(idx);
                break;

            case ItemResource.ItemBehavior.Tool:
                idx = _toolArr.IndexOf(item);
                if (idx < 0) return;
                _toolList.RemoveItem(idx);
                _toolArr.RemoveAt(idx);
                break;
        }
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

        newItem.Init(item);

        newItem.GlobalPosition = _player.GlobalPosition - new Vector3(0, 0.5f, 0);
    }

    // ───────────────────────── EQUIP HIGHLIGHT ─────────────────────────  NEW

    private void RefreshEquipHighlights()
    {
        if (_equipment == null) return;

        ApplyHighlights(_statList, _statArr);
        ApplyHighlights(_useList,  _useArr);
        ApplyHighlights(_toolList, _toolArr);
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
        if (_statPage.Visible) return (_statArr, _statPage);
        if (_usePage.Visible) return (_useArr, _usePage);
        return (_toolArr, _toolPage);
    }

    private void ShowPage(Control page, Control items)
    {
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