using Godot;
using System;
using System.Collections.Generic;

public partial class PauseMenu : Control 
{
	[Signal]
	public delegate void ToggleMouseLockEventHandler();

	[Export] PackedScene MainMenu;
	private Control _buttonPanelContainer;
	private Control _viewFlashcardsPanelContainer;
	private VBoxContainer _flashcardListContainer;
	
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

		if(_panelStack.Count == 0)
			SceneManager.Instance.SetVisibility(SceneNames.PauseMenu, false);
	}

	private void CloseAll(){
		if (_panelStack.Count > 0)
		{
			_panelStack.Pop().Hide();
			_panelStack.Clear();
			SceneManager.Instance.SetVisibility(SceneNames.PauseMenu, false);
		}
	}

	public override void _Ready()
	{
		_buttonPanelContainer = GetNode<Control>("ButtonPanelContainer");
		_viewFlashcardsPanelContainer = GetNode<Control>("ViewFlashcardsPanelContainer");
		_flashcardListContainer = GetNode<VBoxContainer>("ViewFlashcardsPanelContainer/Panel/MarginContainer/VBoxContainer/ScrollContainer/FlashcardListContainer");
	}

	public override void _UnhandledInput(InputEvent @event)
	{
		if (@event.IsActionPressed("ui_cancel"))
		{
			if (_panelStack.Count == 0)
			{
				Visible = true;
				PushPanel(_buttonPanelContainer);
			}
			else
			{
				// Close the top panel
				PopPanel();
			}
		}
	}

	public void _on_resume_pressed()
	{
		GD.Print("Resume Pressed");
		CloseAll();
	}

	public void _on_view_flashcards_pressed()
	{
		PopulateFlashcardList();
		PushPanel(_viewFlashcardsPanelContainer);
	}

	public void _on_flashcards_back_pressed()
	{
		PopPanel();
	}

	private void PopulateFlashcardList()
	{
		foreach (Node child in _flashcardListContainer.GetChildren())
		{
			_flashcardListContainer.RemoveChild(child);
			child.QueueFree();
		}

		if (FlashcardManager.Instance == null || FlashcardManager.Instance.ActiveFlashCardLists == null || FlashcardManager.Instance.ActiveFlashCardLists.Count == 0)
		{
			var emptyLabel = new Label();
			emptyLabel.Text = "No imported flashcards";
			_flashcardListContainer.AddChild(emptyLabel);
			return;
		}

		foreach (FlashcardSet set in FlashcardManager.Instance.ActiveFlashCardLists)
		{
			// Create a horizontal container for the set name and delete button
			var setHeaderContainer = new HBoxContainer();
			_flashcardListContainer.AddChild(setHeaderContainer);

			var setNameLabel = new Label();
			setNameLabel.Text = set.DisplayName ?? "(Unnamed set)";
			setNameLabel.AddThemeFontSizeOverride("font_size", 18);
			setNameLabel.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
			setHeaderContainer.AddChild(setNameLabel);

			// Add delete button next to the set name
			var deleteButton = new Button();
			deleteButton.Text = "Delete";
			deleteButton.CustomMinimumSize = new Vector2(60, 0);
			string setName = set.DisplayName; // Capture for lambda
			deleteButton.Pressed += () => OnDeleteSetPressed(setName);
			setHeaderContainer.AddChild(deleteButton);

			if (set.Cards == null)
				continue;

			foreach (Flashcard card in set.Cards)
			{
				var cardLabel = new Label();
				cardLabel.Text = (string.IsNullOrEmpty(card.Question) ? "(no question)" : card.Question) + "  →  " + (string.IsNullOrEmpty(card.Answer) ? "(no answer)" : card.Answer);
				cardLabel.AutowrapMode = TextServer.AutowrapMode.WordSmart;
				cardLabel.CustomMinimumSize = new Vector2(340, 0);
				_flashcardListContainer.AddChild(cardLabel);
			}

			var spacer = new Control();
			spacer.CustomMinimumSize = new Vector2(0, 12);
			_flashcardListContainer.AddChild(spacer);
		}
	}

	private void OnDeleteSetPressed(string setDisplayName)
	{
		if (FlashcardManager.Instance.DeleteSet(setDisplayName))
		{
			// Refresh the list to reflect the deletion
			PopulateFlashcardList();
		}
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
		// GetTree().ChangeSceneToPacked(MainMenu);
		SceneManager.Instance.Navigate(SceneNames.MainMenu);
		SceneManager.Instance.Free(SceneNames.Dungeon);
		SceneManager.Instance.SetVisibility(SceneNames.PauseMenu, false);
		SceneManager.Instance.Free(SceneNames.PauseMenu);
	}

	public void _on_quit_pressed()
	{
		GD.Print("Quitting Game...");
		GetTree().Quit();
	}
}
