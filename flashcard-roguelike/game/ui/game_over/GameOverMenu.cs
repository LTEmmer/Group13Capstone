using Godot;
using System;

/// <summary>
/// Game over screen displayed when the player dies.
/// Allows the player to restart or return to the main menu.
/// </summary>
public partial class GameOverMenu : Control
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
		// Visible = false;
		
		
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
		
		SceneManager.Instance.SetUI(SceneNames.GameOver);
		// Visible = true;
		Input.MouseMode = Input.MouseModeEnum.Visible;
		GetTree().Paused = true;
	}
	
	public void _on_restart_pressed()
	{
		GD.Print("Restarting game...");
		GetTree().Paused = false;
		SceneManager.Instance.FreeAll();
		SceneManager.Instance.ChangeScene(SceneNames.Dungeon);

		//TODO there is probaly a better way then just redoing all this. It should already be done in the player ready function
		SceneManager.Instance.PreloadUI(SceneNames.PauseMenu_ButtonPanel, true);
		SceneManager.Instance.PreloadUI(SceneNames.PauseMenu_ViewFlashcards, true);
		SceneManager.Instance.PreloadUI(SceneNames.GameOver, true);
		//GetTree().ChangeSceneToFile("res://game/entity/dungeon_generator/dungeon_generator.tscn");
	}
	
	public void _on_main_menu_pressed()
	{
		GD.Print("Returning to main menu...");
		GetTree().Paused = false;

		Input.MouseMode = Input.MouseModeEnum.Visible;
		SceneManager.Instance.FreeAll();
		SceneManager.Instance.SetUI(SceneNames.MainMenu);
		// if (MainMenuScene != null)
		// {
		// 	GetTree().ChangeSceneToPacked(MainMenuScene);
		// }
		// else
		// {
		// 	GetTree().ChangeSceneToFile("res://game/ui/main_menu/main_menu.tscn");
		// }
	}
	
	public void _on_quit_pressed()
	{
		GD.Print("Quitting game...");
		GetTree().Quit();
	}
}
