using Godot;
using System;
using System.Threading;

public partial class PauseMenu : CanvasLayer 
{

	public override void _UnhandledInput(InputEvent @event)
	{
    	if (@event.IsActionPressed("ui_cancel"))
    	{
			this.Visible = (this.Visible == true)? false : true;
    	}
	}
	public void _on_resume_pressed()
	{
		GD.Print("Resume Pressed");
		this.Visible = false;
		
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
	}
	public void _on_quit_pressed()
	{
		GD.Print("Quitting Game...");
        GetTree().Quit();
	}
}
