using Godot;

public partial class AudioManager : Node
{
    public static AudioManager Instance { get; private set; }

    [Export] public AudioStream HoverSound;
    [Export] public AudioStream ClickSound;

    private AudioStreamPlayer _buttonHoverPlayer;
    private AudioStreamPlayer _buttonClickPlayer;

    public override void _Ready()
    {
        Instance = this;

        _buttonHoverPlayer = new AudioStreamPlayer();
        _buttonClickPlayer = new AudioStreamPlayer();
        AddChild(_buttonHoverPlayer);
        AddChild(_buttonClickPlayer);

        _buttonHoverPlayer.Stream = HoverSound;
        _buttonClickPlayer.Stream = ClickSound;
    }

    public void PlayButtonHover() => _buttonHoverPlayer?.Play();
    public void PlayButtonClick() => _buttonClickPlayer?.Play();

    // Wire both signals for a button in one call
    public void RegisterButton(Button button)
    {
        if (button == null) return;
        button.MouseEntered += PlayButtonHover;
        button.Pressed += PlayButtonClick;
    }
}