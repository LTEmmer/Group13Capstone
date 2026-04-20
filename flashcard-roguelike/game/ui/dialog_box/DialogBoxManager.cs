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

    private string _fullText;
    private int _totalVisibleChars = 0;
    private float _charTimer = 0f;
    private bool _isDoneRevealing = true;
    private float _interval;
    private bool _isJustTriggered = false;


    public static DialogBoxManager Instance; 

    public override void _Ready()
    {
        Instance = this;
        Visible = false;

        _yesButton.Pressed += OnYesPressed;
        _noButton.Pressed += OnNoPressed;
    }

    public void ShowDialog(string text, string npcName, Texture2D npcIcon, AudioStream npcVoice, 
                           float charsPerSecond = 10f, bool needsResponse = false)
    {
        _fullText = text;

        _dialogText.Text = _fullText;
        _npcNameLabel.Text = npcName;
        _npcIcon.Texture = npcIcon;
        _voicePlayer.Stream = npcVoice;

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

        Visible = true;
        Input.MouseMode = Input.MouseModeEnum.Visible;

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

            if (_totalVisibleChars % 3 == 0)
            {
                _voicePlayer.PitchScale = 1f + ((float)GD.RandRange(-.25, .25));
                _voicePlayer.Play();
            }
        }
    }

    public void OnYesPressed()
    {
        GD.Print("Yes button pressed");
        Visible = false;
        Input.MouseMode = Input.MouseModeEnum.Captured;
    }

    public void OnNoPressed()
    {
        GD.Print("No button pressed");
        Visible = false;
        Input.MouseMode = Input.MouseModeEnum.Captured;
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
                    Input.MouseMode = Input.MouseModeEnum.Captured;
                }
            }
        }
    }





    /*
    public static DialogBox Instance { get; private set; }

    [Export] private RichTextLabel _dialogText;
    [Export] private Label _npcNameLabel;
    [Export] private Label _continueHint;
    [Export] private Control _buttonRow;
    [Export] private Button _yesButton;
    [Export] private Button _noButton;
    [Export] private AudioStreamPlayer _voicePlayer;

    private string _fullText = "";
    private int _visibleChars = 0;
    private float _charTimer = 0f;
    private bool _isRevealing = false;
    private bool _dialogOpen = false;
    private bool _justOpened = false;
    private float _basePitch = 1.0f;
    private int _charsPerSecond = 10;
    private Tween _blinkTween;
    private Action _onConfirm;
    private Action _onDecline;

    public override void _Ready()
    {
        Instance = this;
        _voicePlayer.Stream = GenerateVoiceBlip();
        _yesButton.Pressed += OnYesPressed;
        _noButton.Pressed += OnNoPressed;
        Visible = false;
    }

    public void ShowDialog(string text, string npcName = "", float voicePitch = 1.0f, int charactersPerSecond = 10, Action onConfirm = null, Action onDecline = null)
    {
        Input.MouseMode = Input.MouseModeEnum.Visible;

        if (_dialogText == null)
        {
            GD.PrintErr("DialogBox: exported node references are null — ensure DialogBox.tscn is registered as an Autoload and all node paths are assigned in the Inspector.");
            return;
        }

        _isRevealing = true;
        _charsPerSecond = charactersPerSecond;
        _fullText = text;
        _visibleChars = 0;
        _basePitch = voicePitch;
        _dialogOpen = true;
        _charTimer = 0f;
        _onConfirm = onConfirm;
        _onDecline = onDecline;
        _dialogText.VisibleCharacters = 0;
        _dialogText.Text = _fullText;
        _npcNameLabel.Text = npcName;
        _continueHint.Visible = false;
        _buttonRow.Visible = false;
        _justOpened = true;
        _blinkTween?.Kill();
        Visible = true;
    }

    public override void _Process(double delta)
    {
        _justOpened = false;

        if (!_isRevealing)
        {
            return;
        }


        _charTimer += (float)delta;
        float interval = 1f / _charsPerSecond;

        if (_charTimer >= interval && _visibleChars < _fullText.Length)
        {
            _charTimer -= interval;
            _visibleChars++;
            _dialogText.VisibleCharacters = _visibleChars;

            char c = _fullText[_visibleChars - 1];
            if (c != ' ' && c != '\n' && !char.IsPunctuation(c))
            {
                _voicePlayer.PitchScale = _basePitch * (0.95f + (float)GD.Randf() * 0.10f);
                _voicePlayer.Play();
            }
        }

        if (_visibleChars >= _fullText.Length)
        {
            _isRevealing = false;
            ShowEndState();
        }
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (!_dialogOpen || _justOpened)
        {
            return;
        }

        if (!@event.IsActionPressed("interact"))
        {
            return;
        }

        if (_isRevealing)
        {
            GD.Print("Skipping to end of dialog");
            _isRevealing = false;
            _visibleChars = _fullText.Length;
            _dialogText.VisibleCharacters = _visibleChars;
            ShowEndState();
        }
        else if (_onConfirm == null)
        {
            // Simple mode: interact closes
            CloseDialog();
        }

        // Confirmation mode: buttons handle it, don't close on interact
        GetViewport().SetInputAsHandled();
    }

    private void ShowEndState()
    {
        if (_onConfirm != null)
        {
            _buttonRow.Visible = true;
            _continueHint.Visible = false;
        }
        else
        {
            _continueHint.Visible = true;
            _blinkTween = CreateTween().SetLoops();
            _blinkTween.TweenProperty(_continueHint, "modulate:a", 0f, 0.5f);
            _blinkTween.TweenProperty(_continueHint, "modulate:a", 1f, 0.5f);
        }
    }

    private void OnYesPressed()
    {
        _onConfirm?.Invoke();
        CloseDialog(restoreMouse: false); // callback owns mouse state from here
    }

    private void OnNoPressed()
    {
        _onDecline?.Invoke();
        CloseDialog(restoreMouse: true);
    }

    private void CloseDialog(bool restoreMouse = true)
    {
        _blinkTween?.Kill();
        _dialogOpen = false;
        _buttonRow.Visible = false;
        Visible = false;
        if (restoreMouse)
            Input.MouseMode = Input.MouseModeEnum.Captured;
    }

    private static AudioStreamWav GenerateVoiceBlip()
    {
        int sampleRate = 22050;
        float duration = 0.07f;
        int samples = (int)(sampleRate * duration);
        byte[] data = new byte[samples * 2];

        for (int i = 0; i < samples; i++)
        {
            float t = (float)i / sampleRate;
            float envelope = Mathf.Sin(Mathf.Pi * (float)i / samples);
            float wave = envelope * Mathf.Sin(2f * Mathf.Pi * 440f * t);
            short s = (short)(wave * 12000);
            data[i * 2]     = (byte)(s & 0xFF);
            data[i * 2 + 1] = (byte)((s >> 8) & 0xFF);
        }

        var stream = new AudioStreamWav();
        stream.Data = data;
        stream.Format = AudioStreamWav.FormatEnum.Format16Bits;
        stream.MixRate = sampleRate;
        stream.Stereo = false;
        return stream;
    } */
}
