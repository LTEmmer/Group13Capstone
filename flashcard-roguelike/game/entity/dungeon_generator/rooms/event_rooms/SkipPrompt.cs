using Godot;

public partial class SkipPrompt : Control
{
	[Signal] public delegate void SkipPressedEventHandler();
	[Signal] public delegate void KeepTryingPressedEventHandler();

	[Export] public Button SkipButton;
	[Export] public Button KeepTryingButton;

	public override void _Ready()
	{
		if (SkipButton != null)
		{
			SkipButton.Pressed += () => EmitSignal(nameof(SkipPressed));
			AudioManager.Instance?.RegisterButton(SkipButton);
		}

		if (KeepTryingButton != null)
		{
			KeepTryingButton.Pressed += () => EmitSignal(nameof(KeepTryingPressed));
			AudioManager.Instance?.RegisterButton(KeepTryingButton);
		}
		
		Visible = false;
	}
}
