using Godot;
using System;

public partial class FlashcardChallenge : Control, IFlashcardChallenge
{
	[Signal]
	public delegate void OnAnswerSubmittedEventHandler(bool isCorrect);

	[Export] public float ShowDuration = 0.3f;
	[Export] public float HideDuration = 0.3f;

	private Control _challengePanel;
	private Label _questionLabel;
	private LineEdit _answerInput;
	private Button _submitButton;
	private Label _contextLabel;
    private Label _answerLabel;

	private Flashcard _currentCard;
	private bool _isActive = false;
	private Action<bool> _onAnswerSubmittedCallback;

	private const float FeedbackDisplayTime = 2.5f; // Seconds to show correct/incorrect feedback before hiding

	public override void _Ready()
	{
		// Get UI elements
		_challengePanel = GetNode<Control>("CenterContainer/ChallengePanel");
		_questionLabel = GetNode<Label>("CenterContainer/ChallengePanel/MarginContainer/VBoxContainer/QuestionScroll/QuestionLabel");
		_answerInput = GetNode<LineEdit>("CenterContainer/ChallengePanel/MarginContainer/VBoxContainer/AnswerInput");
		_contextLabel = GetNode<Label>("CenterContainer/ChallengePanel/MarginContainer/VBoxContainer/ContextLabel");
		_submitButton = GetNode<Button>("CenterContainer/ChallengePanel/MarginContainer/VBoxContainer/SubmitButton");
        _answerLabel = GetNode<Label>("CenterContainer/ChallengePanel/MarginContainer/VBoxContainer/AnswerLabel");

		// Connect signals
		_submitButton.Pressed += OnSubmitPressed;
		_answerInput.TextSubmitted += (text) => OnSubmitPressed();
		AudioManager.Instance?.RegisterButton(_submitButton);

		// Start hidden
		Visible = false;
	}

	public bool Visibility => Visible;

	public void ConnectAnswerSubmitted(Action<bool> callback)
	{
		_onAnswerSubmittedCallback = callback;
	}

	public void ShowChallenge(Flashcard card, string context = "Answer correctly or bad things happen...", bool combat = true)
	{
		if (_isActive || card == null) return; // Already active or no card to show

		_currentCard = card;
		_isActive = true;

		// Setup UI
		_questionLabel.Text = card.Question;
		_contextLabel.Text = context;
		if (combat)
		{
			_contextLabel.AddThemeColorOverride("font_color", Colors.Red);
		}
		else
		{
			_contextLabel.AddThemeColorOverride("font_color", Colors.Blue);
		}

		_answerInput.Text = "";
        _answerLabel.Text = "Your Answer:";
		_answerInput.Editable = true;
		_submitButton.Disabled = false;

		// Show with fade-in animation
		Visible = true;
		Tween tween = CreateTween();
		tween.TweenProperty(_challengePanel, "modulate:a", 1.0f, ShowDuration)
			 .SetTrans(Tween.TransitionType.Quad)
			 .SetEase(Tween.EaseType.Out);

		// Focus the input field
		CallDeferred("FocusAnswerInput");
	}

	private void FocusAnswerInput()
	{
		_answerInput.GrabFocus(); // Ensure the input field is focused for immediate typing
	}

	public void HideChallenge(Action onComplete = null)
	{
		if (!_isActive) return; // Already hidden, no need to hide again

		_isActive = false;
		_answerInput.Editable = false;
		_submitButton.Disabled = true;

		// Hide with fade-out animation using tween, then hide the UI and call onComplete if provided
		Tween tween = CreateTween();
		tween.TweenProperty(_challengePanel, "modulate:a", 0.0f, HideDuration)
			 .SetTrans(Tween.TransitionType.Quad)
			 .SetEase(Tween.EaseType.In);

		tween.TweenCallback(Callable.From(() =>
		{
			Visible = false;
			_currentCard = null;
			onComplete?.Invoke();
		}));
	}

	private void OnSubmitPressed()
	{
		if (!_isActive || _currentCard == null) return; // Not active or no card to check against

        // Check the player's answer against the correct answer, ignoring case and trimming whitespace
		string playerAnswer = _answerInput.Text.Trim();
		string correctAnswer = _currentCard.Answer.Trim();

		// Simple case-insensitive comparison
		bool isCorrect = string.Equals(playerAnswer, correctAnswer, StringComparison.OrdinalIgnoreCase);

		// Play sound
		if (isCorrect)
		{
			AudioManager.Instance?.PlayCorrectSound();
		}
		else
		{
			AudioManager.Instance?.PlayWrongSound();
		}
		
		// Disable input while processing
		_answerInput.Editable = false;
		_submitButton.Disabled = true;

		// Visual feedback
		if (isCorrect)
		{
			_answerInput.Modulate = new Color(0.5f, 1.0f, 0.5f); // Green tint
		}
		else
		{
			_answerInput.Modulate = new Color(1.0f, 0.5f, 0.5f); // Red tint
			_contextLabel.Text = $"Incorrect!";
            _answerLabel.Text = $"Correct Answer: {_currentCard.Answer}";
		}

		// Wait a moment then hide and emit result
        GetTree().CreateTimer(FeedbackDisplayTime).Timeout += () =>
		{
			_answerInput.Modulate = Colors.White;
			HideChallenge(() => 
			{
				_onAnswerSubmittedCallback?.Invoke(isCorrect);
			});
		};
	}
}
