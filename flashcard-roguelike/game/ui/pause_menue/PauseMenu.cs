using Godot;
using System;
using System.Collections.Generic;
using System.Threading;

public partial class PauseMenu : CanvasLayer
{
	private Control _buttonPanelContainer;
	private Control _viewFlashcardsPanelContainer;
	private VBoxContainer _flashcardListContainer;

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
			if (_viewFlashcardsPanelContainer.Visible)
			{
				_on_flashcards_back_pressed();
			}
			else
			{
				Visible = !Visible;
			}
		}
	}

	public void _on_resume_pressed()
	{
		GD.Print("Resume Pressed");
		Visible = false;
	}

	public void _on_view_flashcards_pressed()
	{
		_buttonPanelContainer.Visible = false;
		_viewFlashcardsPanelContainer.Visible = true;
		PopulateFlashcardList();
	}

	public void _on_flashcards_back_pressed()
	{
		_viewFlashcardsPanelContainer.Visible = false;
		_buttonPanelContainer.Visible = true;
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
			var setNameLabel = new Label();
			setNameLabel.Text = set.DisplayName ?? "(Unnamed set)";
			setNameLabel.AddThemeFontSizeOverride("font_size", 18);
			_flashcardListContainer.AddChild(setNameLabel);

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
