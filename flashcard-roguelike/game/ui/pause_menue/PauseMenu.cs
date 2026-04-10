using Godot;
using System;
using System.Collections.Generic;

public partial class PauseMenu : CanvasLayer
{
	[Signal]
	public delegate void ToggleMouseLockEventHandler();

	[Export] PackedScene MainMenu;
	private Control _buttonPanelContainer;
	private SettingsPanel _settingsPanel;
	private Stack<Control> _panelStack = new Stack<Control>();

	/*
	* Methods to handel opening multiple Control node windows from the pause menu
	*/
	private void PushPanel(Control panel)
	{
		// Hide the current top panel if any
		if (_panelStack.Count > 0)
		{
			_panelStack.Peek().Hide();
		}
		
		// Push and show the new panel
		_panelStack.Push(panel);
		panel.Show();
	}

	private void PopPanel()
	{
		if (_panelStack.Count > 0)
		{
			_panelStack.Pop().Hide();
			
			// Show the new top panel if any
			if (_panelStack.Count > 0)
			{
				_panelStack.Peek().Show();
			}
		}
	}

	private void CloseAll(){
		if (_panelStack.Count > 0)
		{
			_panelStack.Pop().Hide();
			_panelStack.Clear();
		}
	}

	public override void _Ready()
	{
		ProcessMode = ProcessModeEnum.Always;
		_buttonPanelContainer = GetNode<Control>("ButtonPanelContainer");
		_settingsPanel = GetNode<SettingsPanel>("SettingsPanelContainer");

		_buttonPanelContainer.ProcessMode = ProcessModeEnum.Always;
		_settingsPanel.ProcessMode = ProcessModeEnum.Always;

		const string btnPath = "ButtonPanelContainer/ButtonPanel/MarginContainer/ButtonContainer/";
		AudioManager.Instance?.RegisterButton(GetNodeOrNull<Button>(btnPath + "Resume"));
		AudioManager.Instance?.RegisterButton(GetNodeOrNull<Button>(btnPath + "Settings"));
		AudioManager.Instance?.RegisterButton(GetNodeOrNull<Button>(btnPath + "Abandon Run"));
		AudioManager.Instance?.RegisterButton(GetNodeOrNull<Button>(btnPath + "Main Menu"));
		AudioManager.Instance?.RegisterButton(GetNodeOrNull<Button>(btnPath + "Quit"));
	}

	public override void _UnhandledInput(InputEvent @event)
	{
		if (@event.IsActionPressed("ui_cancel"))
		{
			if (_panelStack.Count == 0)
			{
				EmitSignal(SignalName.ToggleMouseLock);
				Visible = true;
				PushPanel(_buttonPanelContainer);
				GetTree().Paused = true;
			}
			else
			{
				// Close the top panel
				PopPanel();
				
				if (_panelStack.Count == 0)
				{
					// All panels closed, hide the pause menu
					Visible = false;
					EmitSignal(SignalName.ToggleMouseLock);
					GetTree().Paused = false;
				}
			}
		}
	}

	public void _on_resume_pressed()
	{
		GD.Print("Resume Pressed");
		CloseAll();
		EmitSignal(SignalName.ToggleMouseLock);
		GetTree().Paused = false;
	}

	public void _on_view_flashcards_pressed()
	{
	}

	public void _on_settings_pressed()
	{
		_settingsPanel.SyncFromAudioManager();
		PushPanel(_settingsPanel);
	}

	public void _on_settings_back_pressed()
	{
		PopPanel();
	}

	public void _on_abandon_run_pressed()
	{
		GD.Print("Abandon Run Pressed");
	}

	public void _on_main_menu_pressed()
	{
		GD.Print("Main Menue Pressed");
		GetTree().Paused = false;
		SceneTransition.FadeOut(this, () => GetTree().ChangeSceneToPacked(MainMenu));
	}

	public void _on_quit_pressed()
	{
		GD.Print("Quitting Game...");
		GetTree().Quit();
	}
}
