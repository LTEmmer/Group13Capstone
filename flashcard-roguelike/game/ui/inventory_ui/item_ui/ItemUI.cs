using Godot;

public partial class ItemUI : Control
{
    private ItemInstance _item;
    public ItemInstance Item => _item;

    [Export] private TextureRect _imgTextureRect;
    [Export] private Label _nameLabel;
    [Export] private Label _countLabel;
    [Export] private Label _descriptionLabel;

    public void Init(ItemInstance item)
    {
        _item = item;
        _item.Changed += Refresh;
        Refresh();
    }

    private void Refresh()
    {
        _imgTextureRect.Texture = _item.Resource.Icon;
        _nameLabel.Text = _item.Resource.Name;
        _countLabel.Text = _item.Count > 1 ? $" x{_item.Count}" : "";
        _descriptionLabel.Text = _item.Resource.Description;
    }

    public override void _ExitTree()
    {
        if (_item != null) _item.Changed -= Refresh;
    }
}