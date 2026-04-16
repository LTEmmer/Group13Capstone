using Godot;

public partial class HUD : CanvasLayer
{
	private Label _roomNameLabel;
	private Label _healthValueLabel;
	private TextureProgressBar _healthBar;
	private Player _player;
	private HealthComponent _healthComponent;

	public override void _Ready()
	{
		_roomNameLabel = GetNode<Label>("MarginContainer/VBoxContainer/RoomNameLabel");
		_healthValueLabel = GetNode<Label>("%HealthValueLabel");
		_healthBar = GetNode<TextureProgressBar>("%HealthBar");

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
		if (_healthComponent != null)
		{
			int cur = Mathf.CeilToInt(_healthComponent.CurrentHealth);
			int max = Mathf.CeilToInt(_healthComponent.MaxHealth);
			_healthValueLabel.Text = $"{cur}/{max}";
			_healthBar.MaxValue = _healthComponent.MaxHealth;
			_healthBar.Value = _healthComponent.CurrentHealth;
		}
		else
		{
			_healthValueLabel.Text = "--/--";
			_healthBar.MaxValue = 1.0;
			_healthBar.Value = 0.0;
		}

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
