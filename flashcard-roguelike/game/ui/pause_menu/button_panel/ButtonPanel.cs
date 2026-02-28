using Godot;
using System;

public partial class ButtonPanel : Control 
{
	public override void _UnhandledInput(InputEvent @event)
	{
		if (!Visible) return;
    	if (@event.IsActionPressed("ui_cancel"))
    	{
        	GetViewport().SetInputAsHandled();
        	_on_resume_pressed();
    	}
	}

	public void _on_resume_pressed()
	{
		GD.Print("Resume Pressed");
		SceneManager.Instance.HideUI();
		Player.CaptureMouse();
	}

	public void _on_view_flashcards_pressed()
	{
		SceneManager.Instance.SetUI(SceneNames.PauseMenu_ViewFlashcards);
	}

	public void _on_options_pressed()
	{
		GD.Print("Options Pressed");
	}

	public void _on_abandon_run_pressed()
	{
		GD.Print("Abandon Run Pressed");
	}

	public void _on_main_menu_pressed()
	{
		GD.Print("Main Menu Pressed");
		SceneManager.Instance.FreeAll();
		SceneManager.Instance.SetUI(SceneNames.MainMenu);
	}

	public void _on_quit_pressed()
	{
		GD.Print("Quitting Game...");
		GetTree().Quit();
	}
}