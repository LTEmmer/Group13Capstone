using System;
using Godot;
using Godot.Collections;

public partial class InventoryUI : Node3D
{
	public enum Pages
	{
		Stat,
		Use,
		Tool	
	}

    [Export] private InventoryComponent _inventory;

    [Export] private Control _statPage;
    [Export] private Control _statItems;

    [Export] private Control _toolPage;
    [Export] private Control _toolItems;

    [Export] private Control _usePage;
    [Export] private Control _useItems;

    [Export] private ItemList _statList;
    [Export] private ItemList _useList;
    [Export] private ItemList _toolList;

	private Array<ItemInstance> _statArr = new Array<ItemInstance>();
	private Array<ItemInstance> _useArr = new Array<ItemInstance>();
	private Array<ItemInstance> _toolArr = new Array<ItemInstance>();


    public override void _Ready()
    {
        ProcessMode = ProcessModeEnum.Always;

        _inventory.ItemAdded   += OnItemAdded;
        _inventory.ItemRemoved += OnItemRemoved;

        _statList.ItemSelected += OnItemSelected;
        _useList.ItemSelected += OnItemSelected;
        _toolList.ItemSelected += OnItemSelected;

        foreach (ItemInstance item in _inventory.inv)
            OnItemAdded(item);

        showPage(_toolPage, _toolItems);
        Visible = true;
    }

    private void OnItemAdded(ItemInstance item)
    {
		if (item == null || item.Resource == null) {
			GD.Print("ERROR: OnItemAdded() was given a null item or resource");
			return;
		}

		ItemResource res = item.Resource;
		switch (res.Behavior)
		{
			case ItemResource.ItemBehavior.Stat:
				GD.Print("OnItemAdded: Adding "+ res.Name +" to Stat");
				_statList.AddItem(res.Name, res.Icon);
				_statArr.Add(item);
				break;
			case ItemResource.ItemBehavior.Use:
				GD.Print("OnItemAdded: Adding "+ res.Name +" to Use");
				_useList.AddItem(res.Name, res.Icon);
				_useArr.Add(item);
				break;
			case ItemResource.ItemBehavior.Tool:
				GD.Print("OnItemAdded: Adding "+ res.Name +" to Tool");
				_toolList.AddItem(res.Name, res.Icon);
				_toolArr.Add(item);
				break;
			default:
				GD.Print("InventoryUI.OnItemAdded(): Failed to add item to a list");
				break;
		}
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
				break;
			case ItemResource.ItemBehavior.Use:
				idx = _useArr.IndexOf(item);
        		if (idx < 0) return;
				_useList.RemoveItem(idx);
				break;
			case ItemResource.ItemBehavior.Tool:
				idx = _toolArr.IndexOf(item);
        		if (idx < 0) return;
				_toolList.RemoveItem(idx);
				break;
			default:
				GD.Print("InventoryUI.OnItemAdded(): Failed to add item to a list");
				break;
		}
    }

	private void OnItemSelected(long index)
	{
    	var (arr, page) = getOpenPage();

    	if (index < 0 || index >= arr.Count)
        	return;

    	ItemInstance item = arr[(int)index];

    	GD.Print($"[Inventory] Selected: {item.Resource.Name} | Index: {index}");

    	if (page is InventoryPage<ItemResource> inventoryPage)
    	{
        	inventoryPage.SetItem(item);
    	}
	}

	private (Array<ItemInstance>, Control) getOpenPage()
	{
		if (_statPage.Visible)  return (_statArr, _statPage);
		if (_usePage.Visible)  return (_useArr, _usePage);
		return (_toolArr, _toolPage);
	}

	private void showPage(Control page, Control items)
	{
    	_toolPage.Visible	= false;
    	_toolItems.Visible	= false;

    	_statPage.Visible	= false;
    	_statItems.Visible	= false;

    	_usePage.Visible	= false;
    	_useItems.Visible	= false;


        page.Visible = true;
        items.Visible = true;
	}

	private void setRight(Pages page)
	{
    	switch (page)
    	{
        	case Pages.Stat:
            	showPage(_statPage, _statItems);
            	break;
        	case Pages.Use:
            	showPage(_usePage, _useItems);
            	break;
        	default:
            	showPage(_toolPage, _toolItems);
            	break;
    	}
	}
}