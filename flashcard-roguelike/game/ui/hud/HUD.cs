using Godot;

public partial class HUD : CanvasLayer
{
	[Export] private Label _roomNameLabel;
	[Export] private Label _healthLabel;
	[Export] private Label _shieldlabel;
	private Player _player;
	private HealthComponent _healthComponent;

	public override void _Ready()
	{
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
		{
			
			_healthLabel.Text = $"HP: {_healthComponent.CurrentHealth:F0} / {_healthComponent.MaxHealth:F0}";
			_shieldlabel.Text = $"Shield: {_healthComponent.Shield:F0}";
		}
		else
		{
			_healthLabel.Text = "HP: --";
			_shieldlabel.Text = "Shield: --";
		}

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
