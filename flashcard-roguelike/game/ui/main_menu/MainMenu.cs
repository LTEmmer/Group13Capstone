
using Godot;
using System;

public partial class MainMenu : Control
{
	private Control _mainMenuContainer;
	private Control _uploadPanelContainer;
	private Control _viewFlashcardsPanelContainer;
	private VBoxContainer _flashcardListContainer;

	public override void _Ready()
	{
		AudioManager.Instance?.PlayMainMenuMusic();
		InitializeBackgroundViewport();

		_mainMenuContainer = GetNodeOrNull<Control>("CenterContainer");
		_uploadPanelContainer = GetNodeOrNull<Control>("UploadPanelContainer");
		_viewFlashcardsPanelContainer = GetNodeOrNull<Control>("ViewFlashcardsPanelContainer");
		ApplyDefaultFontToFlashcardsPanel();

		_flashcardListContainer = GetNodeOrNull<VBoxContainer>(
			"ViewFlashcardsPanelContainer/Panel/MarginContainer/VBoxContainer/ScrollContainer/FlashcardListContainer"
		);

		var playButton = GetNodeOrNull<Button>("CenterContainer/WhiteboardPanel/MarginContainer/VBoxContainer/PlayButton");
		var quitButton = GetNodeOrNull<Button>("CenterContainer/WhiteboardPanel/MarginContainer/VBoxContainer/QuitButton");
		var uploadButton = GetNodeOrNull<Button>("CenterContainer/WhiteboardPanel/MarginContainer/VBoxContainer/CardUpload");
		var viewFlashcardsButton = GetNodeOrNull<Button>("CenterContainer/WhiteboardPanel/MarginContainer/VBoxContainer/ViewImportedFlashcards");

		var flashcardsBackButton = GetNodeOrNull<Button>(
			"ViewFlashcardsPanelContainer/Panel/MarginContainer/VBoxContainer/Back"
		);

		if (playButton != null) playButton.Pressed += OnPlayPressed;
		if (quitButton != null) quitButton.Pressed += OnQuitPressed;
		if (uploadButton != null) uploadButton.Pressed += OnOpenUploadScreenPressed;
		if (viewFlashcardsButton != null) viewFlashcardsButton.Pressed += OnViewFlashcardsPressed;
		if (flashcardsBackButton != null) flashcardsBackButton.Pressed += OnFlashcardsBackPressed;

		AudioManager.Instance?.RegisterButton(playButton);
		AudioManager.Instance?.RegisterButton(quitButton);
		AudioManager.Instance?.RegisterButton(uploadButton);
		AudioManager.Instance?.RegisterButton(viewFlashcardsButton);
		AudioManager.Instance?.RegisterButton(flashcardsBackButton);

		if (_uploadPanelContainer != null)
			_uploadPanelContainer.Visible = false;

		if (_viewFlashcardsPanelContainer != null)
			_viewFlashcardsPanelContainer.Visible = false;
	}

	private void OnPlayPressed()
	{
		SceneTransition.FadeOut(this, () =>
			GetTree().ChangeSceneToFile("res://game/entity/dungeon_generator/dungeon_generator.tscn")
		);
	}

	private void OnQuitPressed()
	{
		GetTree().Quit();
	}

	private void OnOpenUploadScreenPressed()
	{
		if (_mainMenuContainer != null)
			_mainMenuContainer.Visible = false;

		if (_viewFlashcardsPanelContainer != null)
			_viewFlashcardsPanelContainer.Visible = false;

		if (_uploadPanelContainer != null)
			_uploadPanelContainer.Visible = true;
	}

	private void OnViewFlashcardsPressed()
	{
		if (_viewFlashcardsPanelContainer == null || _flashcardListContainer == null)
		{
			GD.PushError("Flashcards panel not configured on main menu.");
			return;
		}

		PopulateFlashcardList();

		if (_mainMenuContainer != null)
			_mainMenuContainer.Visible = false;

		if (_uploadPanelContainer != null)
			_uploadPanelContainer.Visible = false;

		_viewFlashcardsPanelContainer.Visible = true;
	}

	private void OnFlashcardsBackPressed()
	{
		if (_viewFlashcardsPanelContainer != null)
			_viewFlashcardsPanelContainer.Visible = false;

		if (_mainMenuContainer != null)
			_mainMenuContainer.Visible = true;
	}

	private void PopulateFlashcardList()
	{
		foreach (Node child in _flashcardListContainer.GetChildren())
		{
			_flashcardListContainer.RemoveChild(child);
			child.QueueFree();
		}

		if (FlashcardManager.Instance == null ||
			FlashcardManager.Instance.ActiveFlashCardLists == null ||
			FlashcardManager.Instance.ActiveFlashCardLists.Count == 0)
		{
			var emptyLabel = new Label();
			emptyLabel.Text = "No imported flashcards";
			_flashcardListContainer.AddChild(emptyLabel);
			return;
		}

		foreach (FlashcardSet set in FlashcardManager.Instance.ActiveFlashCardLists)
		{
			var setHeaderContainer = new HBoxContainer();
			_flashcardListContainer.AddChild(setHeaderContainer);

			var setNameLabel = new Label();
			setNameLabel.Text = set.DisplayName ?? "(Unnamed set)";
			setNameLabel.AddThemeFontSizeOverride("font_size", 18);
			if (ThemeDB.FallbackFont != null)
				setNameLabel.AddThemeFontOverride("font", ThemeDB.FallbackFont);
			setNameLabel.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
			setHeaderContainer.AddChild(setNameLabel);

			var deleteButton = new Button();
			deleteButton.Text = "Delete";
			deleteButton.CustomMinimumSize = new Vector2(60, 0);
			if (ThemeDB.FallbackFont != null)
				deleteButton.AddThemeFontOverride("font", ThemeDB.FallbackFont);
			AudioManager.Instance?.RegisterButton(deleteButton);

			string setName = set.DisplayName;
			deleteButton.Pressed += () => OnDeleteSetPressed(setName);
			setHeaderContainer.AddChild(deleteButton);

			if (set.Cards == null)
				continue;

			foreach (Flashcard card in set.Cards)
			{
				var cardLabel = new Label();
				cardLabel.Text =
					(string.IsNullOrEmpty(card.Question) ? "(no question)" : card.Question)
					+ "  →  " +
					(string.IsNullOrEmpty(card.Answer) ? "(no answer)" : card.Answer);
				if (ThemeDB.FallbackFont != null)
					cardLabel.AddThemeFontOverride("font", ThemeDB.FallbackFont);

				cardLabel.AutowrapMode = TextServer.AutowrapMode.WordSmart;
				cardLabel.CustomMinimumSize = new Vector2(340, 0);
				_flashcardListContainer.AddChild(cardLabel);
			}

			var spacer = new Control();
			spacer.CustomMinimumSize = new Vector2(0, 12);
			_flashcardListContainer.AddChild(spacer);
		}

		ApplyDefaultFontToFlashcardsPanel();
	}

	private void OnDeleteSetPressed(string setDisplayName)
	{
		if (FlashcardManager.Instance.DeleteSet(setDisplayName))
			PopulateFlashcardList();
	}

	private void ApplyDefaultFontToFlashcardsPanel()
	{
		if (_viewFlashcardsPanelContainer == null || ThemeDB.FallbackFont == null)
			return;

		ApplyDefaultFontRecursive(_viewFlashcardsPanelContainer);
	}

	private static void ApplyDefaultFontRecursive(Node node)
	{
		if (node is Label label)
			label.AddThemeFontOverride("font", ThemeDB.FallbackFont);
		else if (node is Button button)
			button.AddThemeFontOverride("font", ThemeDB.FallbackFont);

		foreach (Node child in node.GetChildren())
			ApplyDefaultFontRecursive(child);
	}
}
