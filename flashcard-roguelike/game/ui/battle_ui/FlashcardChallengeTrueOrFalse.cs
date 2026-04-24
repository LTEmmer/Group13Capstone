using Godot;
using System;
using System.Collections.Generic;

public partial class FlashcardChallengeTrueOrFalse : Control, IFlashcardChallenge
{
	private Control _challengePanel;
	private Label _questionLabel;
    private Label _answerLabel;
	private Button _trueButton;
	private Button _falseButton;
	private Label _contextLabel;

	private Flashcard _currentCard;
	private Flashcard _incorrectCard;
    private bool _shownStatementIsCorrect;
	private bool _isActive = false;
	private float _showDuration = 0.3f;
	private float _hideDuration = 0.3f;

	private Action<bool> _onAnswerSubmitted;

	// Shared Random instance avoids creating new Random() on every call
	private static readonly Random _rng = new();

	private const float FeedbackDisplayTime = 2.5f; // Seconds to show correct/incorrect feedback before hiding

    public bool Visibility => Visible;

	public override void _Ready()
	{
		// Get UI elements
		_challengePanel = GetNode<Control>("CenterContainer/ChallengePanel");
		_questionLabel = GetNode<Label>("CenterContainer/ChallengePanel/MarginContainer/VBoxContainer/QuestionLabel");
        _answerLabel = GetNode<Label>("CenterContainer/ChallengePanel/MarginContainer/VBoxContainer/AnswerLabel");
		_contextLabel = GetNode<Label>("CenterContainer/ChallengePanel/MarginContainer/VBoxContainer/ContextLabel");
		_trueButton = GetNode<Button>("CenterContainer/ChallengePanel/MarginContainer/VBoxContainer/ButtonContainer/TrueButton");
		_falseButton = GetNode<Button>("CenterContainer/ChallengePanel/MarginContainer/VBoxContainer/ButtonContainer/FalseButton");

		// Connect signals
		_trueButton.Pressed += () => OnAnswerSelected(true);
		_falseButton.Pressed += () => OnAnswerSelected(false);

		AudioManager.Instance?.RegisterButton(_trueButton);
		AudioManager.Instance?.RegisterButton(_falseButton);

		// Start hidden
		Visible = false;
	}

	public void ConnectAnswerSubmitted(Action<bool> callback)
	{
		_onAnswerSubmitted = callback;
	}

	public void ShowChallenge(Flashcard card, string context = "Answer correctly or bad things happen...", bool combat = true)
	{
		if (_isActive || card == null) return;

		_currentCard = card;
		_isActive = true;

		// Get an incorrect card from another flashcard in the set
		_incorrectCard = GetRandomDifferentCard(card);

        // Set question
        _questionLabel.Text = $"Question:\n{card.Question}";

		// Randomly decide if the statement shown is True or False, 50/50
		bool showTrue = _rng.Next(2) == 0;
        _shownStatementIsCorrect = showTrue; // Track if the shown statement is correct for answer evaluation

		if (showTrue)
		{
			_answerLabel.Text = $"Answer:\n{_currentCard.Answer}";
		}
		else
		{
			_answerLabel.Text = $"Answer:\n{_incorrectCard.Answer}";
		}

		_contextLabel.Text = context;
		if (combat)
		{
			_contextLabel.AddThemeColorOverride("font_color", Colors.Red);
		}
		else
		{
			_contextLabel.AddThemeColorOverride("font_color", Colors.Blue);
		}

		_trueButton.Disabled = false;
		_falseButton.Disabled = false;

		// Show with fade-in animation
		Visible = true;
		Tween tween = CreateTween();
		tween.TweenProperty(_challengePanel, "modulate:a", 1.0f, _showDuration)
			 .SetTrans(Tween.TransitionType.Quad)
			 .SetEase(Tween.EaseType.Out);
	}

	public void HideChallenge(Action onComplete = null)
	{
		if (!_isActive) return;

		_isActive = false;
		_trueButton.Disabled = true;
		_falseButton.Disabled = true;

		// Hide with fade-out animation
		Tween tween = CreateTween();
		tween.TweenProperty(_challengePanel, "modulate:a", 0.0f, _hideDuration)
			 .SetTrans(Tween.TransitionType.Quad)
			 .SetEase(Tween.EaseType.In);

		tween.TweenCallback(Callable.From(() =>
		{
			Visible = false;
			_currentCard = null;
			_incorrectCard = null;
			onComplete?.Invoke();
		}));
	}

	private void OnAnswerSelected(bool selectedTrue)
	{
		if (!_isActive || _currentCard == null) return;

		// Disable buttons
		_trueButton.Disabled = true;
		_falseButton.Disabled = true;

		// Determine if answer is correct
		// The statement shown was either the correct answer or an incorrect one
		bool isCorrect = DetermineIfCorrect(selectedTrue);

		GD.Print($"Player selected {(selectedTrue ? "True" : "False")}. Statement was {(isCorrect ? "Correct" : "Incorrect")}. Answer: {_currentCard.Answer}, Incorrect Option: {_incorrectCard.Answer}, Question: {_currentCard.Question}");

		// Play sound
		if (isCorrect)
		{
			AudioManager.Instance?.PlayCorrectSound();
		}
		else
		{
			AudioManager.Instance?.PlayWrongSound();
		}

		// Visual feedback: only the option the player clicked is highlighted
		var chosen = selectedTrue ? _trueButton : _falseButton;
		if (isCorrect)
		{
			chosen.Modulate = new Color(0.5f, 1.0f, 0.5f);
		}
		else
		{
			chosen.Modulate = new Color(1.0f, 0.5f, 0.5f);
			_contextLabel.Text = "Incorrect! The correct answer for:";
            _answerLabel.Text = $"Answer:\n{_currentCard.Answer}";
		}

		// Wait a moment then hide and emit result
		GetTree().CreateTimer(FeedbackDisplayTime).Timeout += () =>
		{
			_trueButton.Modulate = Colors.White;
			_falseButton.Modulate = Colors.White;
			HideChallenge(() => _onAnswerSubmitted?.Invoke(isCorrect));
		};
	}

	private bool DetermineIfCorrect(bool selectedTrue)
	{
        return _shownStatementIsCorrect == selectedTrue;
	}

	private static Flashcard GetRandomDifferentCard(Flashcard excludeCard)
	{
		List<Flashcard> others = FlashcardManager.Instance?.GetActiveCards();

		if (others == null || others.Count == 0)
		{
			return excludeCard;
		}

		others.Remove(excludeCard);

		if (others.Count == 0)
		{
			return excludeCard;
		}
		
		return others[GD.RandRange(0, others.Count - 1)];
	}
}
