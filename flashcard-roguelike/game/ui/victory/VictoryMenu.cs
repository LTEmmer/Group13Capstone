using Godot;
using System;

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
	
	/// <summary>
	/// Shows the victory screen with an optional custom message.
	/// </summary>
	public void ShowVictory(string message = "Congratulations! You conquered the dungeon!")
	{
		if (_messageLabel != null)
		{
			_messageLabel.Text = message;
		}
		
		Visible = true;
		Input.MouseMode = Input.MouseModeEnum.Visible;
		GetTree().Paused = true;
	}
	
	public void _on_new_run_pressed()
	{
		GD.Print("Starting new run...");
		GetTree().Paused = false;
		GetTree().ChangeSceneToFile("res://game/entity/dungeon_generator/dungeon_generator.tscn");
	}
	
	public void _on_main_menu_pressed()
	{
		GD.Print("Returning to main menu...");
		GetTree().Paused = false;
		
		if (MainMenuScene != null)
		{
			GetTree().ChangeSceneToPacked(MainMenuScene);
		}
		else
		{
			GetTree().ChangeSceneToFile("res://game/ui/main_menu/main_menu.tscn");
		}
	}
	
	public void _on_quit_pressed()
	{
		GD.Print("Quitting game...");
		GetTree().Quit();
	}
}
