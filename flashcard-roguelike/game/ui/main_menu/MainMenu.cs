
using Godot;
using System;

public partial class MainMenu : Control
{
	private const int FlashcardSetTitleFontSize = 32;
	private const int FlashcardQaFontSize = 24;

	private Control _mainMenuContainer;
	private Control _uploadPanelContainer;
	private Control _viewFlashcardsPanelContainer;
	private VBoxContainer _flashcardListContainer;
	private SettingsPanel _settingsPanel;
	private Control _createSetPanelContainer;

	public override void _Ready()
	{
		AudioManager.Instance?.PlayMainMenuMusic();
		InitializeBackgroundViewport();

		_mainMenuContainer = GetNodeOrNull<Control>("CenterContainer");
		_uploadPanelContainer = GetNodeOrNull<Control>("UploadPanelContainer");
		_viewFlashcardsPanelContainer = GetNodeOrNull<Control>("ViewFlashcardsPanelContainer");
		_settingsPanel = GetNodeOrNull<SettingsPanel>("SettingsPanelContainer");
		_createSetPanelContainer = GetNodeOrNull<Control>("CreateSetPanelContainer");

		_flashcardListContainer = GetNodeOrNull<VBoxContainer>(
			"ViewFlashcardsPanelContainer/Panel/MarginContainer/VBoxContainer/ScrollContainer/FlashcardListContainer"
		);

		var playButton = GetNodeOrNull<Button>("CenterContainer/WhiteboardPanel/MarginContainer/VBoxContainer/PlayButton");
		var quitButton = GetNodeOrNull<Button>("CenterContainer/WhiteboardPanel/MarginContainer/VBoxContainer/QuitButton");
		var uploadButton = GetNodeOrNull<Button>("CenterContainer/WhiteboardPanel/MarginContainer/VBoxContainer/CardUpload");
		var viewFlashcardsButton = GetNodeOrNull<Button>("CenterContainer/WhiteboardPanel/MarginContainer/VBoxContainer/ViewImportedFlashcards");
		var flashcardsBackButton = GetNodeOrNull<Button>("ViewFlashcardsPanelContainer/Panel/MarginContainer/VBoxContainer/Back");
		var settingsButton = GetNodeOrNull<Button>("CenterContainer/WhiteboardPanel/MarginContainer/VBoxContainer/SettingsButton");
		var createNewSetButton = GetNodeOrNull<Button>("ViewFlashcardsPanelContainer/Panel/MarginContainer/VBoxContainer/CreateNewSet");

		if (playButton != null) playButton.Pressed += OnPlayPressed;
		if (quitButton != null) quitButton.Pressed += OnQuitPressed;
		if (uploadButton != null) uploadButton.Pressed += OnOpenUploadScreenPressed;
		if (viewFlashcardsButton != null) viewFlashcardsButton.Pressed += OnViewFlashcardsPressed;
		if (flashcardsBackButton != null) flashcardsBackButton.Pressed += OnFlashcardsBackPressed;
		if (settingsButton != null) settingsButton.Pressed += OnSettingsPressed;
		if (createNewSetButton != null) createNewSetButton.Pressed += OnCreateNewSetPressed;

		AudioManager.Instance?.RegisterButton(playButton);
		AudioManager.Instance?.RegisterButton(quitButton);
		AudioManager.Instance?.RegisterButton(uploadButton);
		AudioManager.Instance?.RegisterButton(viewFlashcardsButton);
		AudioManager.Instance?.RegisterButton(flashcardsBackButton);
		AudioManager.Instance?.RegisterButton(settingsButton);
		AudioManager.Instance?.RegisterButton(createNewSetButton);

		if (_uploadPanelContainer != null)
			_uploadPanelContainer.Visible = false;

		if (_viewFlashcardsPanelContainer != null)
			_viewFlashcardsPanelContainer.Visible = false;

		if (_createSetPanelContainer != null)
			_createSetPanelContainer.Visible = false;
	}

	public override void _EnterTree()
	{
		base._EnterTree();
		// Ensure mouse is visible
		Input.MouseMode = Input.MouseModeEnum.Visible;
	}

	private void OnPlayPressed()
	{
		if (FlashcardManager.Instance == null || !FlashcardManager.Instance.HasActiveSet())
		{
			ShowInfoDialog("No Active Flashcard Sets", "Please activate at least one flashcard set before playing.");
			return;
		}

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
		if (FlashcardManager.Instance != null && !FlashcardManager.Instance.HasActiveSet())
		{
			var dialog = new ConfirmationDialog();
			dialog.Title = "No Active Sets";
			dialog.DialogText = "No flashcard sets are active. You won't be able to play without one.\nGo back anyway?";
			dialog.OkButtonText = "Go Back";
			ApplyWhiteBackground(dialog);
			AddChild(dialog);
			dialog.PopupCentered();
			dialog.Confirmed += () => { dialog.QueueFree(); NavigateBackFromFlashcards(); };
			dialog.Canceled += () => dialog.QueueFree();
			return;
		}

		NavigateBackFromFlashcards();
	}

	private void NavigateBackFromFlashcards()
	{
		if (_viewFlashcardsPanelContainer != null)
			_viewFlashcardsPanelContainer.Visible = false;

		if (_mainMenuContainer != null)
			_mainMenuContainer.Visible = true;
	}

	private void ShowInfoDialog(string title, string message)
	{
		var dialog = new AcceptDialog();
		dialog.Title = title;
		dialog.DialogText = message;
		ApplyWhiteBackground(dialog);
		AddChild(dialog);
		dialog.PopupCentered();
		dialog.Confirmed += () => dialog.QueueFree();
		dialog.Canceled += () => dialog.QueueFree();
	}

	private static void ApplyWhiteBackground(Window dialog)
	{
		var style = new StyleBoxFlat();
		style.BgColor = Colors.White;
		style.SetContentMarginAll(16f);
		dialog.AddThemeStyleboxOverride("panel", style);
	}

	private void OnCreateNewSetPressed()
	{
		if (_viewFlashcardsPanelContainer != null)
			_viewFlashcardsPanelContainer.Visible = false;

		if (_createSetPanelContainer != null)
			_createSetPanelContainer.Visible = true;
	}

	public void OnCreateSetSaved()
	{
		if (_createSetPanelContainer != null)
			_createSetPanelContainer.Visible = false;

		PopulateFlashcardList();

		if (_viewFlashcardsPanelContainer != null)
			_viewFlashcardsPanelContainer.Visible = true;
	}

	public void OnCreateSetCancelled()
	{
		if (_createSetPanelContainer != null)
			_createSetPanelContainer.Visible = false;

		if (_viewFlashcardsPanelContainer != null)
			_viewFlashcardsPanelContainer.Visible = true;
	}

	private void OnSettingsPressed()
	{
		if (_settingsPanel == null)
		{
			GD.PushError("Settings panel not configured on main menu.");
			return;
		}

		_settingsPanel.SyncFromAudioManager();

		if (_mainMenuContainer != null)
		{
			_mainMenuContainer.Visible = false;
		}

		if (_uploadPanelContainer != null)
		{
			_uploadPanelContainer.Visible = false;
		}

		_settingsPanel.Visible = true;
	}

	private void OnSettingsBackPressed()
	{
		if (_settingsPanel != null)
		{
			_settingsPanel.Visible = false;
		}

		if (_mainMenuContainer != null)
		{
			_mainMenuContainer.Visible = true;
		}
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
			emptyLabel.AddThemeFontSizeOverride("font_size", FlashcardQaFontSize);
			_flashcardListContainer.AddChild(emptyLabel);
			return;
		}

		foreach (FlashcardSet set in FlashcardManager.Instance.ActiveFlashCardLists)
		{
			var setHeaderContainer = new HBoxContainer();
			_flashcardListContainer.AddChild(setHeaderContainer);

			var activeCheckBox = new CheckBox()
			{
				ButtonPressed = set.IsActive,
			};
			string checkSetName = set.DisplayName;
			activeCheckBox.Toggled += (pressed) => FlashcardManager.Instance.SetActive(checkSetName, pressed);
			setHeaderContainer.AddChild(activeCheckBox);

			var setNameLabel = new Label();
			setNameLabel.Text = set.DisplayName ?? "(Unnamed set)";
			setNameLabel.AddThemeFontSizeOverride("font_size", FlashcardSetTitleFontSize);
			setNameLabel.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
			setHeaderContainer.AddChild(setNameLabel);

			var deleteButton = new Button();
			deleteButton.Text = "Delete";
			deleteButton.CustomMinimumSize = new Vector2(60, 0);
			AudioManager.Instance?.RegisterButton(deleteButton);

			string setName = set.DisplayName;
			deleteButton.Pressed += () => OnDeleteSetPressed(setName);
			setHeaderContainer.AddChild(deleteButton);

			if (set.Cards == null)
				continue;

			int cardIndex = 0;

			foreach (Flashcard card in set.Cards)
			{
				cardIndex++;
				var cardLabel = new Label();
				cardLabel.Text =
					$"Card {cardIndex}: " +
					(string.IsNullOrEmpty(card.Question) ? "(no question)" : card.Question)
					+ "  →  " +
					(string.IsNullOrEmpty(card.Answer) ? "(no answer)" : card.Answer);
				cardLabel.AddThemeFontSizeOverride("font_size", FlashcardQaFontSize);

				cardLabel.AutowrapMode = TextServer.AutowrapMode.WordSmart;
				cardLabel.CustomMinimumSize = new Vector2(620, 0);
				_flashcardListContainer.AddChild(cardLabel);
			}

			var spacer = new Control();
			spacer.CustomMinimumSize = new Vector2(0, 24);
			_flashcardListContainer.AddChild(spacer);
		}

		GetNodeOrNull<Control>("ViewFlashcardsPanelContainer/Panel")?.UpdateMinimumSize();
	}

	private void OnDeleteSetPressed(string setDisplayName)
	{
		if (FlashcardManager.Instance.DeleteSet(setDisplayName))
			PopulateFlashcardList();
	}
}
