using Godot;

public partial class UsePage : InventoryPage<ItemResource>
{
    [Export] private Button _useButton;
    [Export] private Button _dropButton;
    [Export] private Label _uses;

    private new ItemInstance _item;

    public override void _Ready()
    {
        _useButton.Pressed  += OnUsePressed;
        _dropButton.Pressed += OnDropPressed;
    }

    public override void SetItem(ItemInstance item)
    {
        base.SetItem(item);
        _item = item;
        _useButton.Visible = !item.Resource.OnlyCombat;
        _dropButton.Disabled = false;
        RefreshUses();
    }

    private void OnUsePressed()
    {
        if (_item == null) return;
        Use(_item);
        RefreshUses();
    }

    private void OnDropPressed()
    {
        if (_item == null) return;
        Drop(_item);
        _item = null;
        _dropButton.Disabled = true;
        _useButton.Disabled  = true;
    }

    private void RefreshUses()
    {
        if (_item == null) return;

        bool limited = _item.Resource.MaxUses > 0;
        _uses.Text           = limited ? _item.CurrentUses.ToString() : "∞";
        _useButton.Disabled  = limited && _item.CurrentUses <= 0;
        _dropButton.Disabled = limited && _item.CurrentUses <= 0;
    }
}