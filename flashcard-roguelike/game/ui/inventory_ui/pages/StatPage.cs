using Godot;

public partial class StatPage : InventoryPage<ItemResource>
{
    [Export] private Button _dropButton;

    private new ItemInstance _item;

    public override void _Ready()
    {

        _dropButton.Pressed += OnDropPressed;
    }

    public override void SetItem(ItemInstance item)
    {
        base.SetItem(item);
        _item = item;
        _dropButton.Disabled = false;
    }

    private void OnDropPressed()
    {
        if (BattleManager.Instance.IsInCombat)
        {
           return; 
        }
        GD.Print("test");
        if (_item == null) return;
        GD.Print("dropping");
        Drop(_item);
        _dropButton.Disabled = true;
    }
}