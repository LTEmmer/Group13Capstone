using Godot;

public partial class HUD : CanvasLayer
{
	[Export] private Label _roomNameLabel;
	[Export] private Label _healthLabel;
	[Export] private Label _shieldlabel;
	[Export] private VBoxContainer _minimapContainer;

	private Label _healthValueLabel;
	private TextureProgressBar _healthBar;
	private Player _player;
	private HealthComponent _healthComponent;

	public override void _Ready()
	{
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

		// Initialize the minimap now that the dungeon graph is fully built
		// (DungeonGenerator._Ready completes before the player and the HUD is added to the tree)
		Minimap minimap = GetNodeOrNull<Minimap>("MinimapContainer/MinimapPanel/MarginContainer/Minimap");
		if (minimap != null)
		{
			DungeonGenerator gen = GetTree().Root.GetNodeOrNull<DungeonGenerator>("DungeonGenerator");
			if (gen?.Graph != null)
			{
				minimap.Initialize(gen.Graph);
			}
		}
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
      		_shieldlabel.Text = $"Shield: {_healthComponent.Shield:F0}";
		}
		else
		{
			_healthValueLabel.Text = "--/--";
			_healthBar.MaxValue = 1.0;
			_healthBar.Value = 0.0;
      		_shieldlabel.Text = "Shield: --";
		}

		if (CurrentRoomManager.Instance != null && CurrentRoomManager.Instance.CurrentRoomId >= 0)
		{
			DungeonGenerator gen = GetTree().Root.GetNodeOrNull<DungeonGenerator>("DungeonGenerator");
			if (gen != null)
				_roomNameLabel.Text = gen.GetRoomDisplayName(CurrentRoomManager.Instance.CurrentRoomId);
			else
				_roomNameLabel.Text = "--";
		}
		else
			_roomNameLabel.Text = "--";
	}

    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event.IsActionPressed("inventory_toggle"))
		{
			var tween = CreateTween();
			tween.TweenProperty(_minimapContainer, "modulate:a", 1f, 0);
			_minimapContainer.Visible = true;
		}
		else if (@event.IsActionReleased("inventory_toggle"))
		{
			var tween = CreateTween();
			tween.TweenProperty(_minimapContainer, "modulate:a", 0f, 1f).SetTrans(Tween.TransitionType.Sine).SetDelay(2f);
			tween.TweenCallback(Callable.From(() =>_minimapContainer.Visible = false));
		}
	}

}
