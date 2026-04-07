using Godot;

public partial class InventoryUI : Node3D
{
    [Export] private InventoryComponent _inventory;
    [Export] private ItemList _itemList;
    [Export] Texture2D testImg;

    // Right page sub-scenes — instance these inside RightPage SubViewport
    [Export] private EquipmentPage  _equipmentPage;
    [Export] private StatPage       _statPage;
    [Export] private ConsumablePage _consumablePage;

    private readonly System.Collections.Generic.List<ItemInstance> _items = new();

    public override void _Ready()
    {
        ProcessMode = ProcessModeEnum.Always;

        _inventory.ItemAdded   += OnItemAdded;
        _inventory.ItemRemoved += OnItemRemoved;
        foreach (var item in _inventory.Items)
            OnItemAdded(item);

        _itemList.ItemSelected += OnItemSelected;

        ShowPage(null); // hide all pages until something is selected
        Visible = true;
    }

    private void OnItemAdded(ItemInstance item)
    {
        _items.Add(item);
        _itemList.AddItem("TEST", testImg);
    }

    private void OnItemRemoved(ItemInstance item)
    {
        int idx = _items.IndexOf(item);
        if (idx < 0) return;
        _items.RemoveAt(idx);
        _itemList.RemoveItem(idx);
    }

    private void OnItemSelected(long index)
    {
        var item = _items[(int)index];
        GD.Print($"[Inventory] Selected: {item} | Index: {index}");
    }

    private void ShowPage(Control page)
    {
        _equipmentPage.Visible  = _equipmentPage  == page;
        _statPage.Visible       = _statPage       == page;
        _consumablePage.Visible = _consumablePage == page;
    }
}