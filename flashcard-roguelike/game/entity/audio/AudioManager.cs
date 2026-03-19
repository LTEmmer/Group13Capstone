using System.Collections.Generic;
using Godot;

public partial class AudioManager : Node
{
    public static AudioManager Instance { get; private set; }

    [Export] public AudioStream HoverSound;
    [Export] public AudioStream ClickSound;

    [Export] public AudioStream CorrectSound;
    [Export] public AudioStream WrongSound;

    [Export] public AudioStream GameOverSound;

    private AudioStreamPlayer _buttonSoundsPlayer;
    private AudioStreamPlayer _correctSoundsPlayer;
    private AudioStreamPlayer _gameConditionsPlayer;

    public override void _Ready()
    {
        Instance = this;

        _buttonSoundsPlayer = new AudioStreamPlayer();
        _correctSoundsPlayer = new AudioStreamPlayer();
        _gameConditionsPlayer = new AudioStreamPlayer();
        AddChild(_buttonSoundsPlayer);
        AddChild(_correctSoundsPlayer);
        AddChild(_gameConditionsPlayer);

    }

    // Wire both signals for a button in one call
    public void RegisterButton(Button button)
    {
        if (button == null) return;
        button.MouseEntered += PlayButtonHover;
        button.Pressed += PlayButtonClick;
    }

    public void PlayButtonHover()
    {
        if (HoverSound != null)
        {
            _buttonSoundsPlayer.Stream = HoverSound;
            _buttonSoundsPlayer.Play();
        }
    }

    public void PlayButtonClick()
    {
        if (ClickSound != null)
        {
            _buttonSoundsPlayer.Stream = ClickSound;
            _buttonSoundsPlayer.Play();
        }
    }

    public void PlayCorrectSound()
    {
        if (CorrectSound != null)
        {
            _correctSoundsPlayer.Stream = CorrectSound;
            _correctSoundsPlayer.Play();
        }
    }

    public void PlayWrongSound()
    {
        if (WrongSound != null)
        {
            _correctSoundsPlayer.Stream = WrongSound;
            _correctSoundsPlayer.Play();
        }
    }

    public void PlayGameOverSound()
    {
        if (GameOverSound != null)
        {
            _gameConditionsPlayer.Stream = GameOverSound;
            _gameConditionsPlayer.Play();
        }
    }
}