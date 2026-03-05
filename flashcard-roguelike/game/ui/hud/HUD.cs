using Godot;

public partial class HUD : CanvasLayer
{
	private Label _roomNameLabel;
	private Label _healthLabel;
	private Player _player;
	private HealthComponent _healthComponent;

	public override void _Ready()
	{
		_roomNameLabel = GetNode<Label>("MarginContainer/VBoxContainer/RoomNameLabel");
		_healthLabel = GetNode<Label>("MarginContainer/VBoxContainer/HealthLabel");

		// HUD is under Camera3D -> CameraPivot -> Player
		Node parent = GetParent();
		if (parent != null)
			parent = parent.GetParent();
		if (parent != null)
			parent = parent.GetParent();
		_player = parent as Player;
		if (_player != null)
			_healthComponent = _player.GetNodeOrNull<HealthComponent>("HealthComponent");
	}

	public override void _Process(double delta)
	{
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
