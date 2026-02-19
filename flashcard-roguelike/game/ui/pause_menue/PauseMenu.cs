using Godot;
using System;
using System.Threading;

public partial class PauseMenu : CanvasLayer 
{
	[Export] PackedScene MainMenu;
	[Signal] delegate void ToggleMouseLockEventHandler();
	public void _on_resume_pressed()
	{
		GD.Print("Resume Pressed");
		EmitSignal(SignalName.ToggleMouseLock);
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
		GD.Print("Main Menue Pressed");
		GetTree().ChangeSceneToPacked(MainMenu);
	}
	public void _on_quit_pressed()
	{
		GD.Print("Quitting Game...");
        GetTree().Quit();
	}
}
