using Godot;
using System;

public partial class MainMenu : Control
{
	private LineEdit _setNameInput;

	public override void _Ready()
	{
		var playButton   = GetNodeOrNull<Button>("CenterContainer/VBoxContainer/PlayButton");
		var quitButton   = GetNodeOrNull<Button>("CenterContainer/VBoxContainer/QuitButton");
		var uploadButton = GetNodeOrNull<Button>("CenterContainer/VBoxContainer/CardUpload");

		_setNameInput = GetNodeOrNull<LineEdit>("CenterContainer/VBoxContainer/SetNameInput");

		if (playButton != null)   playButton.Pressed += OnPlayPressed;
		if (quitButton != null)   quitButton.Pressed += OnQuitPressed;
		if (uploadButton != null) uploadButton.Pressed += OnUploadPressed;

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
}
