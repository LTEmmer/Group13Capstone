using System.Collections.Generic;
using Godot;
using GodotPlugins.Game;

public partial class AudioManager : Node
{
    public static AudioManager Instance { get; private set; }

    // UI sounds
    [Export] public AudioStream HoverSound;
    [Export] public AudioStream ClickSound;
    [Export] public AudioStream[] ItemPickupSounds;

    // Flashcard feedback
    [Export] public AudioStream CorrectSound;
    [Export] public AudioStream WrongSound;

    // Game condition sounds
    [Export] public AudioStream GameOverSound;
    [Export] public AudioStream GameVictorySound;

    // Music tracks
    [Export] public AudioStream MainMenuMusic;
    [Export] public AudioStream[] AmbientMusic;
    [Export] public AudioStream BattleMusic;

    // Battle stinger — plays on top of music when entering battle
    [Export] public AudioStream BattleStinger;

    [Export] public float SoundReduction = -12.5f; // Currently does nothing

    private AudioStreamPlayer _buttonSoundsPlayer;
    private AudioStreamPlayer _correctSoundsPlayer;
    private AudioStreamPlayer _gameConditionsPlayer;
    private AudioStreamPlayer _itemPickupSoundsPlayer;
    private AudioStreamPlayer _stingerPlayer;

    // Two music players for crossfading
    private AudioStreamPlayer _musicPlayerA;
    private AudioStreamPlayer _musicPlayerB;
    private bool _usingPlayerA = true;



    public override void _Ready()
    {
        Instance = this;
        ProcessMode = ProcessModeEnum.Always;

        _buttonSoundsPlayer = GetNode<AudioStreamPlayer>("ButtonSounds");
        _correctSoundsPlayer = GetNode<AudioStreamPlayer>("CorrectSounds");
        _gameConditionsPlayer = GetNode<AudioStreamPlayer>("GameConditions");
        _itemPickupSoundsPlayer = GetNode<AudioStreamPlayer>("ItemPickupSounds");
        _stingerPlayer = GetNode<AudioStreamPlayer>("Stinger");
        _musicPlayerA = GetNode<AudioStreamPlayer>("MusicPlayerA");
        _musicPlayerB = GetNode<AudioStreamPlayer>("MusicPlayerB");
    }

    // --- Music ---
    /// Crossfades from the currently playing music track to a new one.
    public void TransitionToMusic(AudioStream newTrack, float fadeDuration = 1.0f)
    {
        if (newTrack == null) return;

        var fadeOut = _usingPlayerA ? _musicPlayerA : _musicPlayerB;
        var fadeIn  = _usingPlayerA ? _musicPlayerB : _musicPlayerA;

        fadeIn.Stream = newTrack;
        fadeIn.VolumeDb = -80f;
        fadeIn.Play();

        var tween = CreateTween();
        tween.SetParallel(true);
        tween.TweenProperty(fadeOut, "volume_db", -80f, fadeDuration);
        tween.TweenProperty(fadeIn,  "volume_db", 0, fadeDuration);
        tween.Chain().TweenCallback(Callable.From(() =>
        {
            fadeOut.Stop();
        }));

        _usingPlayerA = !_usingPlayerA;
    }

    public void PlayMainMenuMusic(float fadeDuration = 1.0f)
    {
        TransitionToMusic(MainMenuMusic, fadeDuration);
    }

    public void PlayDungeonMusic(float fadeDuration = 1.0f)
    {
        if (AmbientMusic == null || AmbientMusic.Length == 0) return;
        TransitionToMusic(AmbientMusic[GD.Randi() % (uint)AmbientMusic.Length], fadeDuration);
    }

    public void PlayBattleMusic(float fadeDuration = 0.5f)
    {
        TransitionToMusic(BattleMusic, fadeDuration);
    }

    /// Plays the one-shot stinger sound on top of whatever music is currently playing.
    /// Call this when entering battle, alongside PlayBattleMusic.
    public void PlayBattleStinger()
    {
        if (BattleStinger == null) return;
        _stingerPlayer.Stream = BattleStinger;
        _stingerPlayer.Play();
    }

    // --- UI sounds ---

    public void RegisterButton(Button button)
    {
        if (button == null) return;
        button.MouseEntered += () => PlayButtonHover(button);
        button.Pressed += PlayButtonClick;
    }

    public void PlayButtonHover(Button button = null)
    {
        if (button?.Disabled == true) 
        {
            return;
        }

        if (_buttonSoundsPlayer.Stream == ClickSound && _buttonSoundsPlayer.Playing) return;
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

    public void PlayItemPickupSound()
    {
        if (ItemPickupSounds.Length > 0)
        {
            _itemPickupSoundsPlayer.Stream = ItemPickupSounds[GD.Randi() % (uint)ItemPickupSounds.Length];
            _itemPickupSoundsPlayer.Play();
        }
    }

    // --- Flashcard feedback ---

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

    // --- Game conditions ---

    public void PlayGameOverSound()
    {
        if (GameOverSound != null)
        {
            _gameConditionsPlayer.Stream = GameOverSound;
            _gameConditionsPlayer.Play();
        }
    }

    public void PlayGameVictorySound()
    {
        if (GameVictorySound != null)
        {
            _gameConditionsPlayer.Stream = GameVictorySound;
            _gameConditionsPlayer.Play();
        }
    }
}
