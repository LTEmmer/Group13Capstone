using Godot;

public partial class ItemUI : Control
{
    private ItemResource _item;
    public ItemResource Item => _item;

    [Export] private TextureRect _imgTextureRect;
    [Export] private Label _nameLabel;
    [Export] private Label _countLabel;
    [Export] private Label _descriptionLabel;

    public void Init(ItemResource item)
    {
        _item = item;
        _item.Changed += Refresh;
        Refresh();
    }

    private void Refresh()
    {
        _imgTextureRect.Texture = _item.Icon;
        _nameLabel.Text = _item.Name;
        _descriptionLabel.Text = _item.Description;
    }

    public override void _ExitTree()
    {
        if (_item != null) _item.Changed -= Refresh;
    }
}