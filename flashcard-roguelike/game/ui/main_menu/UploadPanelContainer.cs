using Godot;
using System;

public partial class UploadPanelContainer : Control
{
	private LineEdit _pathInput;
	private Button _browseButton;
	private Button _importButton;
	private Button _backButton;
	private Button _instructionsButton;
	private FileDialog _csvFileDialog;
	private Control _instructionsPanel;

	public override void _Ready()
	{
		_pathInput = GetNodeOrNull<LineEdit>("WhiteboardPanel/MarginContainer/VBoxContainer/LineEdit");
		_browseButton = GetNodeOrNull<Button>("WhiteboardPanel/MarginContainer/VBoxContainer/BrowseButton");
		_importButton = GetNodeOrNull<Button>("WhiteboardPanel/MarginContainer/VBoxContainer/ImportButton");
		_backButton = GetNodeOrNull<Button>("WhiteboardPanel/MarginContainer/VBoxContainer/BackButton");
		_instructionsButton = GetNodeOrNull<Button>("WhiteboardPanel/MarginContainer/VBoxContainer/InstructionsButton");
		_csvFileDialog = GetNodeOrNull<FileDialog>("CsvFileDialog");
		_instructionsPanel = GetParent().GetNodeOrNull<Control>("InstructionsPanelContainer");

		var closeButton = _instructionsPanel?.GetNodeOrNull<Button>("Panel/MarginContainer/VBoxContainer/CloseButton");
		if (closeButton != null)
			closeButton.Pressed += () => _instructionsPanel.Visible = false;

		if (_browseButton != null)
		{
			_browseButton.Pressed += OnBrowsePressed;
		}

		if (_importButton != null)
		{
			_importButton.Pressed += OnImportPressed;
		}

		if (_backButton != null)
		{
			_backButton.Pressed += OnBackPressed;
		}

		if (_instructionsButton != null)
		{
			_instructionsButton.Pressed += OnInstructionsPressed;
		}

		AudioManager.Instance?.RegisterButton(_browseButton);
		AudioManager.Instance?.RegisterButton(_importButton);
		AudioManager.Instance?.RegisterButton(_backButton);
		AudioManager.Instance?.RegisterButton(_instructionsButton);
		AudioManager.Instance?.RegisterButton(closeButton);

		if (_csvFileDialog != null)
		{
			_csvFileDialog.FileMode = FileDialog.FileModeEnum.OpenFile;
			_csvFileDialog.Access = FileDialog.AccessEnum.Filesystem;
			_csvFileDialog.UseNativeDialog = true;
			_csvFileDialog.Filters = new string[] { "*.csv ; CSV Files" };
			_csvFileDialog.FileSelected += OnCsvFileSelected;
		}

		if (_pathInput != null)
			_pathInput.Editable = false;

		Visible = false;
	}

	private void OnBrowsePressed()
	{
		if (_csvFileDialog == null)
		{
			GD.PushError("CsvFileDialog not found.");
			return;
		}

		_csvFileDialog.PopupCenteredRatio();
	}

	private void OnCsvFileSelected(string path)
	{
		if (_pathInput != null)
			_pathInput.Text = path;
	}

	private void OnImportPressed()
	{
		string csvPath = _pathInput?.Text?.Trim() ?? "";

		if (string.IsNullOrWhiteSpace(csvPath))
		{
			GD.PushError("No CSV path entered.");
			return;
		}

		if (!csvPath.EndsWith(".csv", StringComparison.OrdinalIgnoreCase))
		{
			GD.PushError("Selected file is not a CSV.");
			return;
		}

		string setName = System.IO.Path.GetFileNameWithoutExtension(csvPath);

		var manager = GetNodeOrNull<FlashcardManager>("/root/FlashcardManager");
		if (manager == null)
		{
			GD.PushError("No /root/FlashcardManager.");
			return;
		}

		GD.Print($"[UI] Calling manager.ImportAndSave(path={csvPath}, setName={setName})");
		manager.ImportAndSave(csvPath, setName);

		Visible = false;

		var mainMenu = GetParent().GetNodeOrNull<Control>("CenterContainer");
		if (mainMenu != null)
			mainMenu.Visible = true;
	}

	private void OnBackPressed()
	{
		Visible = false;

		var mainMenu = GetParent().GetNodeOrNull<Control>("ViewFlashcardsPanelContainer");
		if (mainMenu != null)
			mainMenu.Visible = true;
	}

	private void OnInstructionsPressed()
	{
		if (_instructionsPanel != null)
			_instructionsPanel.Visible = true;
	}
}
