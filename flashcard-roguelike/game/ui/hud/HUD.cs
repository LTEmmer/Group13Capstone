using Godot;

public partial class HUD :	Control 
{
	private Label _roomNameLabel;
	private Label _healthLabel;
	[Export] PackedScene _playerScene;
	private Player _player;
	private HealthComponent _healthComponent;

	public override void _Ready()
	{
		_roomNameLabel = GetNode<Label>("MarginContainer/VBoxContainer/RoomNameLabel");
		_healthLabel = GetNode<Label>("MarginContainer/VBoxContainer/HealthLabel");
	}

	public void SetPlayer(Player player)
	{
		_player = player;
		_healthComponent = _player.GetNodeOrNull<HealthComponent>("HealthComponent");
	}

	public override void _Process(double delta)
	{
		// TODO: this could be done with an update signal when health changes
		// Health
		if (_healthComponent != null)
			_healthLabel.Text = $"HP: {_healthComponent.CurrentHealth:F0} / {_healthComponent.MaxHealth:F0}";
		else
			_healthLabel.Text = "HP: --";

		// Room name
		if (CurrentRoomManager.Instance != null && CurrentRoomManager.Instance.CurrentRoomId >= 0)
		{
			DungeonGenerator gen = GetTree().Root.GetNodeOrNull<DungeonGenerator>("Dungeon");
			if (gen != null)
				_roomNameLabel.Text = gen.GetRoomDisplayName(CurrentRoomManager.Instance.CurrentRoomId);
			else
				_roomNameLabel.Text = "--";
		}
		else
			_roomNameLabel.Text = "--";
	}
}
