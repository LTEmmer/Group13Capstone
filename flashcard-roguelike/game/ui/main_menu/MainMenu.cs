
using Godot;
using System;

public partial class MainMenu : Control
{
	private Control _mainMenuContainer;
	private Control _uploadPanelContainer;
	private Control _viewFlashcardsPanelContainer;
	private VBoxContainer _flashcardListContainer;

	private Camera3D[] _cameras;
	private SpotLight3D _light;
	private int _activeCameraIndex = 0;

	private static readonly Color[] LightColors = 
	{
		new Color(1.0f, 0.2f, 0.2f),  // neon red
		new Color(1.0f, 0.5f, 0.0f),  // vivid orange
		new Color(1.0f, 0.9f, 0.1f),  // bright yellow
		new Color(0.2f, 1.0f, 0.3f),  // electric green
		new Color(0.0f, 1.0f, 0.9f),  // aqua cyan
		new Color(0.2f, 0.6f, 1.0f),  // vibrant blue
		new Color(0.6f, 0.3f, 1.0f),  // neon purple
		new Color(1.0f, 0.2f, 0.8f),  // magenta
		new Color(1.0f, 0.0f, 0.6f),  // hot pink
		new Color(0.4f, 1.0f, 0.8f),  // mint neon
		new Color(0.8f, 1.0f, 0.2f),  // lime
		new Color(0.1f, 0.4f, 1.0f),  // deep electric blue
	};

	// (position, look-at target) pairs for spotlight placement
	private static readonly (Vector3 pos, Vector3 target)[] LightAngles = {
		(new Vector3( 0.0f, 3.5f,  1.5f), new Vector3( 0,   1,    0)),  // front-top
		(new Vector3( 2.0f, 3.0f,  1.0f), new Vector3( 0,   1,    0)),  // right-top
		(new Vector3(-2.0f, 3.0f,  1.0f), new Vector3( 0,   1,    0)),  // left-top
		(new Vector3( 0.0f, 3.5f, -1.5f), new Vector3( 0,   1,    0)),  // back-top
		(new Vector3( 2.5f, 2.0f,  0.0f), new Vector3( 0,   1,    0)),  // far right
		(new Vector3(-2.5f, 2.0f,  0.0f), new Vector3( 0,   1,    0)),  // far left
		(new Vector3( 1.5f, 4.0f,  1.5f), new Vector3( 0, 0.5f,   0)),  // high front-right
		(new Vector3(-1.5f, 4.0f,  1.5f), new Vector3( 0, 0.5f,   0)),  // high front-left
	};

	public override void _Ready()
	{
		AudioManager.Instance?.PlayMainMenuMusic();

		var animPlayer = GetNodeOrNull<AnimationPlayer>(
			"BackgroundViewport/SubViewport/AnimationLibrary_Godot_Standard/AnimationPlayer");
		if (animPlayer != null)
		{
			animPlayer.SpeedScale = 1.2f;
			animPlayer.Play("Dance");
			animPlayer.AnimationFinished += _ => animPlayer.Play("Dance");
		}

		// Camera cycling setup
		var vp = GetNodeOrNull<SubViewport>("BackgroundViewport/SubViewport");
		if (vp != null)
		{
			_cameras = new Camera3D[] {
				vp.GetNodeOrNull<Camera3D>("Cam1"),
				vp.GetNodeOrNull<Camera3D>("Cam2"),
				vp.GetNodeOrNull<Camera3D>("Cam3"),
				vp.GetNodeOrNull<Camera3D>("Cam4"),
				vp.GetNodeOrNull<Camera3D>("Cam5"),
				vp.GetNodeOrNull<Camera3D>("Cam6"),
				vp.GetNodeOrNull<Camera3D>("Cam7"),
				vp.GetNodeOrNull<Camera3D>("Cam8"),
			};
			_light = vp.GetNodeOrNull<SpotLight3D>("SpotLight3D");
		}

		// Activate a random starting camera
		SwitchCamera();
		var cameraTimer = new Timer { WaitTime = 2, Autostart = true };
		cameraTimer.Timeout += SwitchCamera;
		AddChild(cameraTimer);

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

	private void SwitchCamera()
	{
		if (_cameras == null) return;

		// Pick a different camera than the current one
		int next;
		do { next = GD.RandRange(0, _cameras.Length - 1); }
		while (_cameras.Length > 1 && next == _activeCameraIndex);

		if (_cameras[_activeCameraIndex] != null)
			_cameras[_activeCameraIndex].Current = false;

		_activeCameraIndex = next;

		if (_cameras[_activeCameraIndex] != null)
			_cameras[_activeCameraIndex].Current = true;

		// Random light color and angle
		if (_light != null)
		{
			_light.LightColor = LightColors[GD.RandRange(0, LightColors.Length - 1)];

			var (pos, target) = LightAngles[GD.RandRange(0, LightAngles.Length - 1)];
			_light.Position = pos;
			_light.LookAt(target);
		}
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
