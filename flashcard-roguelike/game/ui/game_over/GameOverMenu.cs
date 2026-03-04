using Godot;
using System;

/// <summary>
/// Game over screen displayed when the player dies.
/// Allows the player to restart or return to the main menu.
/// </summary>
public partial class GameOverMenu : CanvasLayer
{
	[Export]
	public PackedScene MainMenuScene { get; set; }
	
	[Export]
	public PackedScene GameScene { get; set; }
	
	private Control _panel;
	private Label _titleLabel;
	private Label _messageLabel;
	
	public override void _Ready()
	{
		_panel = GetNodeOrNull<Control>("Panel");
		_titleLabel = GetNodeOrNull<Label>("Panel/VBoxContainer/TitleLabel");
		_messageLabel = GetNodeOrNull<Label>("Panel/VBoxContainer/MessageLabel");
		
		// Start hidden
		Visible = false;
		
		// Ensure mouse is visible when game over shows
		ProcessMode = ProcessModeEnum.Always;
	}
	
	/// <summary>
	/// Shows the game over screen with an optional custom message.
	/// </summary>
	public void ShowGameOver(string message = "You have fallen in the dungeon...")
	{
		if (_messageLabel != null)
		{
			_messageLabel.Text = message;
		}
		
		Visible = true;
		Input.MouseMode = Input.MouseModeEnum.Visible;
		GetTree().Paused = true;
	}
	
	public void _on_restart_pressed()
	{
		GD.Print("Restarting game...");
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
