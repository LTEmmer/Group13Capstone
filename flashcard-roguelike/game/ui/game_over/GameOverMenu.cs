using Godot;
using System.Collections.Generic;

/// <summary>
/// Game over screen displayed when the player dies.
/// Shows the tracked session stats and returns to the main menu on input.
/// </summary>
public partial class GameOverMenu : CanvasLayer
{
	[Export]
	public PackedScene MainMenuScene { get; set; }

	public bool PendingShow { get; set; }
	public string PendingMessage { get; set; } = "Run summary";
	public bool PendingPlayAudio { get; set; }

	private const string MarkerFontPath = "res://assets/fonts/DryWhiteboardMarker-Regular.ttf";

	private FontFile _markerFont;
	private Label _messageLabel;
	private Label _statsHeaderLabel;
	private GridContainer _statsListContainer;
	private bool _advanceRequested;
	private bool _showVictory;

	public override void _Ready()
	{
		_markerFont = GD.Load<FontFile>(MarkerFontPath);
		_messageLabel = GetNodeOrNull<Label>("CenterContainer/MainPanel/MarginContainer/MainVBox/MessageLabel");
		_statsHeaderLabel = GetNodeOrNull<Label>("CenterContainer/MainPanel/MarginContainer/MainVBox/StatsHeaderLabel");
		_statsListContainer = GetNodeOrNull<GridContainer>("CenterContainer/MainPanel/MarginContainer/MainVBox/StatsListContainer");

		ApplyMarkerFontRecursive(this);

		Visible = false;
		ProcessMode = ProcessModeEnum.Always;

		if (GetTree().CurrentScene == this)
		{
			ShowGameOver();
			return;
		}

		if (PendingShow)
		{
			PendingShow = false;
			ShowStatsScreen(PendingMessage, PendingPlayAudio);
		}
	}

	public override void _Input(InputEvent @event)
	{
		if (!_advanceRequested)
		{
			return;
		}

		if (@event is InputEventMouseButton mouseButton && mouseButton.Pressed)
		{
			GoToMainMenu();
		}
		else if (@event is InputEventKey key && key.Pressed && !key.Echo)
		{
			GoToMainMenu();
		}
		else if (@event is InputEventScreenTouch touch && touch.Pressed)
		{
			GoToMainMenu();
		}
	}

	/// <summary>
	/// Shows the game over screen with an optional custom message.
	/// </summary>
	public void ShowGameOver(string message = "You have fallen in the dungeon...")
	{
		if (!IsNodeReady())
		{
			CallDeferred("ShowGameOver", message);
			return;
		}

		ShowStatsScreen(message, true);
	}

	/// <summary>
	/// Shows the same end-of-run stats screen from non-death flows such as the pause menu.
	/// </summary>
	public void ShowSessionStats(string message = "Run summary", bool playAudio = false, bool showVictory = false)
	{
		if (!IsNodeReady())
		{
			PendingShow = true;
			PendingMessage = message;
			PendingPlayAudio = playAudio;
			return;
		}

		ShowStatsScreen(message, playAudio, showVictory);
	}

	private void ShowStatsScreen(string message, bool playAudio, bool showVictory = false)
	{
		_showVictory = showVictory;

		if (_messageLabel != null)
		{
			_messageLabel.Text = message;
		}

		RefreshStats();

		BattleManager.Instance?.Transitions?.Hide();
		BattleManager.Instance?.ActiveUI?.Hide();

		if (!showVictory)
		{
			AudioManager.Instance?.FadeOutMusic();
			if (playAudio)
			{
				AudioManager.Instance?.PlayGameOverSound();
			}
		}

		_advanceRequested = true;
		Visible = true;
		Input.MouseMode = Input.MouseModeEnum.Visible;
		GetTree().Paused = true;
	}

	private void RefreshStats()
	{
		ClearContainer(_statsListContainer);

		foreach (TaloTelemetry.SessionStatEntry stat in TaloTelemetry.GetSessionStats())
		{
			AddStatCard(_statsListContainer, stat.Label, stat.Value);
		}
	}

	private void GoToMainMenu()
	{
		if (!_advanceRequested)
		{
			return;
		}

		_advanceRequested = false;

		if (_showVictory)
		{
			Visible = false;
			return;
		}

		SceneTransition.FadeOut(this, () =>
		{
			GetTree().Paused = false;
			if (MainMenuScene != null)
			{
				GetTree().ChangeSceneToPacked(MainMenuScene);
			}
			else
			{
				GetTree().ChangeSceneToFile("res://game/ui/main_menu/main_menu.tscn");
			}

			QueueFree();
		});
	}

	private void AddStatCard(Container parent, string label, string value)
	{
		if (parent == null)
		{
			return;
		}

		var card = new PanelContainer();
		card.CustomMinimumSize = new Vector2(500, 62);
		card.AddThemeStyleboxOverride("panel", CreateCardStyle());
		parent.AddChild(card);

		var margin = new MarginContainer();
		margin.AddThemeConstantOverride("margin_left", 12);
		margin.AddThemeConstantOverride("margin_top", 8);
		margin.AddThemeConstantOverride("margin_right", 12);
		margin.AddThemeConstantOverride("margin_bottom", 8);
		card.AddChild(margin);

		var row = new HBoxContainer();
		row.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
		row.AddThemeConstantOverride("separation", 10);
		margin.AddChild(row);

		var labelNode = new Label
		{
			Text = label,
			SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
			AutowrapMode = TextServer.AutowrapMode.WordSmart
		};
		labelNode.AddThemeFontSizeOverride("font_size", 15);
		labelNode.AddThemeColorOverride("font_color", new Color(0.06f, 0.06f, 0.06f));
		if (_markerFont != null)
		{
			labelNode.AddThemeFontOverride("font", _markerFont);
		}
		row.AddChild(labelNode);

		var valueNode = new Label
		{
			Text = value,
			HorizontalAlignment = HorizontalAlignment.Right,
			VerticalAlignment = VerticalAlignment.Center,
			CustomMinimumSize = new Vector2(88, 0)
		};
		valueNode.AddThemeFontSizeOverride("font_size", 20);
		valueNode.AddThemeColorOverride("font_color", new Color(0.06f, 0.06f, 0.06f));
		if (_markerFont != null)
		{
			valueNode.AddThemeFontOverride("font", _markerFont);
		}
		row.AddChild(valueNode);
	}

	private static void ClearContainer(Container container)
	{
		if (container == null)
		{
			return;
		}

		foreach (Node child in container.GetChildren())
		{
			container.RemoveChild(child);
			child.QueueFree();
		}
	}

	private void ApplyMarkerFontRecursive(Node node)
	{
		if (_markerFont == null || node == null)
		{
			return;
		}

		if (node is Label label)
		{
			label.AddThemeFontOverride("font", _markerFont);
		}

		foreach (Node child in node.GetChildren())
		{
			ApplyMarkerFontRecursive(child);
		}
	}

	private static StyleBoxFlat CreateCardStyle()
	{
		return new StyleBoxFlat
		{
			ContentMarginLeft = 14,
			ContentMarginTop = 14,
			ContentMarginRight = 14,
			ContentMarginBottom = 14,
			BgColor = new Color(0.995f, 0.994f, 0.99f, 0.95f),
			BorderWidthLeft = 2,
			BorderWidthTop = 2,
			BorderWidthRight = 2,
			BorderWidthBottom = 2,
			BorderColor = new Color(0.22f, 0.22f, 0.2f, 0.4f),
			CornerRadiusTopLeft = 6,
			CornerRadiusTopRight = 6,
			CornerRadiusBottomLeft = 6,
			CornerRadiusBottomRight = 6,
			ShadowColor = new Color(0, 0, 0, 0.12f),
			ShadowSize = 3,
			ShadowOffset = new Vector2(1, 2)
		};
	}

}
