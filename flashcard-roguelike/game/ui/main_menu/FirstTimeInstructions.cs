using Godot;

public partial class FirstTimeInstructions : Control
{
	private const string FlagPath = "user://first_launch_seen";
	private CheckBox _neverShowAgainChecked;

	public override void _Ready()
	{
		if (FileAccess.FileExists(FlagPath))
		{
			QueueFree();
			return;
		}

		var closeButton = GetNodeOrNull<Button>("CenterContainer/Panel/MarginContainer/VBoxContainer/CloseButton");
		_neverShowAgainChecked = GetNodeOrNull<CheckBox>("CenterContainer/Panel/MarginContainer/VBoxContainer/CheckBox");
		AudioManager.Instance.RegisterButton(closeButton);
		
		if (closeButton != null)
		{
			closeButton.Pressed += OnClose;
		}
	}

	private void OnClose()
	{
		if (_neverShowAgainChecked.ButtonPressed)
		{
			using var file = FileAccess.Open(FlagPath, FileAccess.ModeFlags.Write);
		}
		
		QueueFree();
	}
}
