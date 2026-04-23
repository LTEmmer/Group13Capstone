using Godot;
using System;

public partial class BattleUI : Control
{
	[Signal]
	public delegate void OnActionSelectedEventHandler(string action);

	[Export] public float SlideInDuration = 0.5f;
	[Export] public float SlideOutDuration = 0.3f;

	private Control _actionPanel;
	private Button _attackButton;
	private Button _runButton;
	private Button _itemsButton;
	private Label _playerHealthLabel;
	private Label _combatLogLabel;

	private Vector2 _hiddenPosition;
	private Vector2 _visiblePosition;
	private bool _isVisible = false;

	public override void _Ready()
	{
		// Get UI elements
		_actionPanel = GetNode<Control>("ActionPanel");
		_attackButton = GetNode<Button>("ActionPanel/MarginContainer/ActionsContainer/AttackButton");
		_runButton = GetNode<Button>("ActionPanel/MarginContainer/ActionsContainer/RunButton");
		_itemsButton = GetNode<Button>("ActionPanel/MarginContainer/ActionsContainer/ItemsButton");
		_playerHealthLabel = GetNode<Label>("StatusPanel/MarginContainer/VBoxContainer/PlayerStatus/HealthLabel");
		_combatLogLabel = GetNode<Label>("CombatLogPanel/MarginContainer/VBoxContainer/ScrollContainer/LogMargin/LogLabel");

		// Connect button signals
		_attackButton.Pressed += () => OnActionButtonPressed("attack");
		_runButton.Pressed += () => OnActionButtonPressed("run");
		_itemsButton.Pressed += () => OnActionButtonPressed("items");

		AudioManager.Instance?.RegisterButton(_attackButton);
		AudioManager.Instance?.RegisterButton(_runButton);
		AudioManager.Instance?.RegisterButton(_itemsButton);

		// Setup positions for sliding animation
		float screenHeight = GetViewportRect().Size.Y;
		_visiblePosition = new Vector2(_actionPanel.Position.X, screenHeight - _actionPanel.Size.Y);
		_hiddenPosition = new Vector2(_actionPanel.Position.X, screenHeight);
		_actionPanel.Position = _hiddenPosition;

		// Start hidden
		Visible = false;
	}

	private void OnActionButtonPressed(string action)
	{
        // Emit the selected action and disable buttons until next turn
		EmitSignal(SignalName.OnActionSelected, action);
		SetActionsEnabled(false);
	}

	public void SlideIn()
	{
		if (_isVisible) return; // Already visible, no need to slide in

		_isVisible = true;
		Visible = true;

        // Animate the panel sliding up from the bottom with tween
		Tween tween = CreateTween();
		tween.TweenProperty(_actionPanel, "position", _visiblePosition, SlideInDuration)
			 .SetTrans(Tween.TransitionType.Back)
			 .SetEase(Tween.EaseType.Out);
	}

	public void SlideOut(Action onComplete = null)
	{
		if (!_isVisible) return; // Already hidden, no need to slide out

		_isVisible = false;

        // Animate the panel sliding down off-screen with tween, then hide the UI and call onComplete if provided
		Tween tween = CreateTween();
		tween.TweenProperty(_actionPanel, "position", _hiddenPosition, SlideOutDuration)
			 .SetTrans(Tween.TransitionType.Quad)
			 .SetEase(Tween.EaseType.In);

		if (onComplete != null)
		{
			tween.TweenCallback(Callable.From(() =>
			{
				Visible = false;
				onComplete.Invoke();
			}));
		}
	}

	public void SetActionsEnabled(bool enabled)
	{
        // Enable or disable action buttons based on the current state (e.g., player's turn or waiting for input)
		_attackButton.Disabled = !enabled;
		_runButton.Disabled = !enabled;
		_itemsButton.Disabled = !enabled;
	}

	public void UpdatePlayerHealth(float current, float max)
	{
		_playerHealthLabel.Text = $"Player HP: {Mathf.Ceil(current)}/{max}";
	}

	public void AddCombatLog(string message)
	{
        // Add a timestamp to each log entry and append it to the combat log label,
        // also limit the number of lines to prevent overflow

		string timestamp = DateTime.Now.ToString("HH:mm:ss");
		_combatLogLabel.Text += $"\n[{timestamp}] {message}";

		// Limit log length to prevent overflow
		string[] lines = _combatLogLabel.Text.Split('\n');
		if (lines.Length > 10)
		{
			_combatLogLabel.Text = string.Join("\n", lines[(lines.Length - 10)..]);
		}
	}

	public void ClearCombatLog()
	{
		_combatLogLabel.Text = "Combat Started!";
	}
}
