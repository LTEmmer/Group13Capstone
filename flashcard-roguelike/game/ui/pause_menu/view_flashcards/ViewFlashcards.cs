using Godot;
using System;

public partial class ViewFlashcards : Control
{
	[Export] private VBoxContainer _flashcardListContainer;

	public override void _Ready()
	{
		PopulateFlashcardList();
	}

	public override void _UnhandledInput(InputEvent @event)
	{
		if (!Visible) return;
		if (@event.IsActionPressed("ui_cancel"))
		{
			GetViewport().SetInputAsHandled();
			_on_back_pressed();
		}
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
			var emptyLabel = new Label { Text = "No imported flashcards" };
			_flashcardListContainer.AddChild(emptyLabel);
			return;
		}

		foreach (FlashcardSet set in FlashcardManager.Instance.ActiveFlashCardLists)
		{
			var setHeaderContainer = new HBoxContainer();
			_flashcardListContainer.AddChild(setHeaderContainer);

			var setNameLabel = new Label { Text = set.DisplayName ?? "(Unnamed set)" };
			setNameLabel.AddThemeFontSizeOverride("font_size", 18);
			setNameLabel.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
			setHeaderContainer.AddChild(setNameLabel);

			var deleteButton = new Button { Text = "Delete", CustomMinimumSize = new Vector2(60, 0) };
			string setName = set.DisplayName;
			deleteButton.Pressed += () => OnDeleteSetPressed(setName);
			setHeaderContainer.AddChild(deleteButton);

			if (set.Cards == null) continue;

			foreach (Flashcard card in set.Cards)
			{
				var cardLabel = new Label
				{
					Text = (string.IsNullOrEmpty(card.Question) ? "(no question)" : card.Question) + "  →  " + (string.IsNullOrEmpty(card.Answer) ? "(no answer)" : card.Answer),
					AutowrapMode = TextServer.AutowrapMode.WordSmart,
					CustomMinimumSize = new Vector2(340, 0)
				};
				_flashcardListContainer.AddChild(cardLabel);
			}

			var spacer = new Control { CustomMinimumSize = new Vector2(0, 12) };
			_flashcardListContainer.AddChild(spacer);
		}
	}

	private void OnDeleteSetPressed(string setDisplayName)
	{
		if (FlashcardManager.Instance.DeleteSet(setDisplayName))
			PopulateFlashcardList();
	}

	private void _on_back_pressed()
	{
		SceneManager.Instance.SetUI(SceneNames.PauseMenu_ButtonPanel);
	}
}