using System;
using System.Runtime.InteropServices;
using System.Transactions;
using Godot;

public partial class DialogBoxManager : CanvasLayer
{
    [Export] private AudioStreamPlayer _voicePlayer;
    [Export] private Label _npcNameLabel;
    [Export] private TextureRect _npcIcon;
    [Export] private RichTextLabel _dialogText;
    [Export] private Button _yesButton;
    [Export] private Button _noButton;
    [Export] private Label _continueText;
    [Export] private int _speakEveryXChars = 3;

    private string _fullText;
    private int _totalVisibleChars = 0;
    private float _charTimer = 0f;
    private bool _isDoneRevealing = true;
    private float _interval;
    private bool _isJustTriggered = false;
    private Action _onYes;
    private Action _onNo;
    private Input.MouseModeEnum _savedMouseMode;
    private AudioStream[] _voices;


    public static DialogBoxManager Instance; 

    public override void _Ready()
    {
        Instance = this;
        Visible = false;

        _yesButton.Pressed += OnYesPressed;
        _noButton.Pressed += OnNoPressed;
    }

    public void ShowDialog(string text, string npcName, Texture2D npcIcon, AudioStream[] npcVoices,
                           float charsPerSecond = 10f, bool needsResponse = false, Action onYes = null, Action onNo = null)
    {
        if (Visible) 
        {
            return;
        }

        _fullText = text;

        _dialogText.Text = _fullText;
        _npcNameLabel.Text = npcName;
        _npcIcon.Texture = npcIcon;
        _onYes = onYes;
        _onNo = onNo;
        _voices = npcVoices;

        _isDoneRevealing = false;
        _totalVisibleChars = 0;
        _charTimer = 0f;
        _interval = 1f / charsPerSecond;

        _dialogText.VisibleCharacters = 0;
        _dialogText.Text = _fullText;

        if (needsResponse)
        {
            _yesButton.Visible = true;
            _noButton.Visible = true;
            _continueText.Visible = false;
        }
        else
        {
            _yesButton.Visible = false;
            _noButton.Visible = false;
            _continueText.Visible = true;
        }

        _savedMouseMode = Input.MouseMode;
        Input.MouseMode = Input.MouseModeEnum.Visible;
        Visible = true;
        _isJustTriggered = true;
    }

    public override void _Process(double delta)
    {
        if (_isDoneRevealing)
        {
            return;
        }

        if (_isJustTriggered)
        {
            _isJustTriggered = false;
        }

        _charTimer += (float)delta;

        if (_charTimer >= _interval)
        {
            _dialogText.VisibleCharacters++;
            _totalVisibleChars++;
            _charTimer = 0f;

            if (_totalVisibleChars >= _fullText.Length)
            {
                _isDoneRevealing = true;
            }

            if (_totalVisibleChars % _speakEveryXChars == 0)
            {
                _voicePlayer.Stream = _voices[GD.RandRange(0, _voices.Length - 1)];
                _voicePlayer.PitchScale = 1f + ((float)GD.RandRange(-.25, .25));
                _voicePlayer.Play();
            }
        }
    }

    public void OnYesPressed()
    {
        ButtonPressed();
        _onYes?.Invoke();
    }

    public void OnNoPressed()
    {
        ButtonPressed();
        _onNo?.Invoke();
    }

    private void ButtonPressed()
    {
        Visible = false;
        Input.MouseMode = _savedMouseMode; 
        _isDoneRevealing = true;
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (Visible)
        {
            if (@event.IsActionPressed("interact") && !_isJustTriggered)
            {
                if (!_isDoneRevealing)
                {
                    _dialogText.VisibleCharacters = _fullText.Length;
                    _isDoneRevealing = true;
                }
                else if (_continueText.Visible)
                {
                    Visible = false;
                    Input.MouseMode = _savedMouseMode;
                }
            }
        }
    }
}