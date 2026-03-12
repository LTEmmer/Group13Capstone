using Godot;
using System;

public partial class MainMenu : Control
{
	private LineEdit _setNameInput;
	private Control _mainMenuContainer;
	private Control _viewFlashcardsPanelContainer;
	private VBoxContainer _flashcardListContainer;

	public override void _Ready()
	{
		_mainMenuContainer = GetNodeOrNull<Control>("CenterContainer");

		var playButton   = GetNodeOrNull<Button>("CenterContainer/VBoxContainer/PlayButton");
		var quitButton   = GetNodeOrNull<Button>("CenterContainer/VBoxContainer/QuitButton");
		var uploadButton = GetNodeOrNull<Button>("CenterContainer/VBoxContainer/CardUpload");
		var viewFlashcardsButton = GetNodeOrNull<Button>("CenterContainer/VBoxContainer/ViewImportedFlashcards");

		_viewFlashcardsPanelContainer = GetNodeOrNull<Control>("ViewFlashcardsPanelContainer");
		_flashcardListContainer = GetNodeOrNull<VBoxContainer>("ViewFlashcardsPanelContainer/Panel/MarginContainer/VBoxContainer/ScrollContainer/FlashcardListContainer");

		_setNameInput = GetNodeOrNull<LineEdit>("CenterContainer/VBoxContainer/SetNameInput");

		if (playButton != null)           playButton.Pressed += OnPlayPressed;
		if (quitButton != null)           quitButton.Pressed += OnQuitPressed;
		if (uploadButton != null)         uploadButton.Pressed += OnUploadPressed;
		if (viewFlashcardsButton != null) viewFlashcardsButton.Pressed += OnViewFlashcardsPressed;

		var backButton = GetNodeOrNull<Button>("ViewFlashcardsPanelContainer/Panel/MarginContainer/VBoxContainer/Back");
		if (backButton != null) backButton.Pressed += OnFlashcardsBackPressed;

		GD.Print("MainMenu _Ready ran");
		GD.Print($"SetNameInput null? {_setNameInput == null}");
	}

	private void OnPlayPressed()
	{
		GetTree().ChangeSceneToFile("res://game/entity/dungeon_generator/dungeon_generator.tscn");
	}

	private void OnQuitPressed()
	{
		GetTree().Quit();
	}

	private void OnUploadPressed()
	{
		GD.Print("Upload button pressed");
		string csvPath = _setNameInput?.Text?.Trim() ?? "";

		if (string.IsNullOrEmpty(csvPath))
		{
			GD.PushError("No CSV path entered.");
			return;
		}

		string setName = System.IO.Path.GetFileNameWithoutExtension(csvPath);

		var manager = GetNodeOrNull<Node>("/root/FlashcardManager");
		GD.Print($"Manager found? {manager != null}");

		if (manager == null)
		{
			GD.PushError("No /root/FlashcardManager.");
			return;
		}

		GD.Print($"[UI] Calling manager.LoadSetFromCsv(path={csvPath}, setName={setName})");
		manager.Call("LoadSetFromCsv", csvPath, setName);
		GD.Print($"Sent to FlashcardManager: {setName}");
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
		{
			_mainMenuContainer.Visible = false;
		}
		_viewFlashcardsPanelContainer.Visible = true;
	}

	private void OnFlashcardsBackPressed()
	{
		if (_viewFlashcardsPanelContainer != null)
		{
			_viewFlashcardsPanelContainer.Visible = false;
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

		if (FlashcardManager.Instance == null || FlashcardManager.Instance.ActiveFlashCardLists == null || FlashcardManager.Instance.ActiveFlashCardLists.Count == 0)
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
			setNameLabel.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
			setHeaderContainer.AddChild(setNameLabel);

			var deleteButton = new Button();
			deleteButton.Text = "Delete";
			deleteButton.CustomMinimumSize = new Vector2(60, 0);
			string setName = set.DisplayName;
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
			PopulateFlashcardList();
		}
	}
}
