using Godot;
using System;
using System.Collections;

/// <summary>
/// Victory screen displayed when the player completes the dungeon.
/// Allows the player to start a new run or return to the main menu.
/// </summary>
public partial class VictoryMenu : CanvasLayer
{
	[Export]
	public PackedScene MainMenuScene { get; set; }
	
	private Control _panel;
	private Label _titleLabel;
	private Label _messageLabel;
	
	public override void _Ready()
	{
		_panel = GetNodeOrNull<Control>("Panel");
		_titleLabel = GetNodeOrNull<Label>("Panel/VBoxContainer/TitleLabel");
		_messageLabel = GetNodeOrNull<Label>("Panel/VBoxContainer/MessageLabel");

		// Ensure mouse is visible when victory shows
		ProcessMode = ProcessModeEnum.Always;

		// If running this scene directly (for preview), show immediately
		if (GetTree().CurrentScene == this)
		{
			ShowVictory();
		}
		else
		{
			// Start hidden when loaded as part of another scene
			Visible = false;
		}
	}

	public void ShowVictory(string message = null)
	{
		int floor = GameDifficultyManager.Instance?.CurrentFloor ?? 1;

		if (_titleLabel != null)
			_titleLabel.Text = $"FLOOR {floor} COMPLETE!";

		if (_messageLabel != null)
			_messageLabel.Text = message ?? $"You cleared Floor {floor}! Proceed deeper?";

		AudioManager.Instance?.FadeOutMusic();
		AudioManager.Instance?.PlayGameVictorySound();

		Visible = true;
		Input.MouseMode = Input.MouseModeEnum.Visible;
		GetTree().Paused = true;
	}

	public void _on_next_floor_pressed()
	{
		GD.Print("Advancing to next floor...");
		GetTree().Paused = false;
		Visible = false;
		DungeonGenerator gen = GetTree().GetFirstNodeInGroup("dungeon_generator") as DungeonGenerator;
		gen?.GoToNextFloor();
	}

	public void _on_new_run_pressed()
	{
		GD.Print("Starting new run...");
		GameDifficultyManager.Instance?.ResetGame();
		GetTree().Paused = false;
		GetTree().ChangeSceneToFile("res://game/entity/dungeon_generator/dungeon_generator.tscn");
		Visible = false;
	}

	public void _on_main_menu_pressed()
	{
		GD.Print("Returning to main menu...");
		GameDifficultyManager.Instance?.ResetGame();
		GetTree().Paused = false;

		if (MainMenuScene != null)
		{
			GetTree().ChangeSceneToPacked(MainMenuScene);
		}
		else
		{
			GetTree().ChangeSceneToFile("res://game/ui/main_menu/main_menu.tscn");
		}

		Visible = false;
	}
	
	public void _on_quit_pressed()
	{
		GD.Print("Quitting game...");
		GetTree().Quit();
	}
}
