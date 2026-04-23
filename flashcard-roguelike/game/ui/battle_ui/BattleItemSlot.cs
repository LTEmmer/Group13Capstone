using Godot;
using System;

public partial class BattleItemSlot : VBoxContainer
{
	[Export] private Label _nameLabel;
	[Export] private Button _spriteButton;
	[Export] private Label _usesLabel;

	public void Init(ItemInstance item, int index, Theme tooltipTheme, Action<int> onClicked)
	{
		_nameLabel.Text = item.Resource.Name;

		_spriteButton.Icon = item.Resource.Icon;
		_spriteButton.TooltipText = item.Resource.Description;
		_spriteButton.Theme = tooltipTheme;
		AudioManager.Instance?.RegisterButton(_spriteButton);
		_spriteButton.Pressed += () => onClicked(index);

		bool limited = item.Resource.MaxUses > 0;
		_usesLabel.Text = limited ? $"{item.CurrentUses}/{item.Resource.MaxUses}" : "∞";
	}

	public void SetSelected(bool selected)
	{
		_spriteButton.Modulate = selected ? new Color(0.7f, 0.9f, 1f, 1f) : Colors.White;
	}
}
