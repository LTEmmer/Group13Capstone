using Godot;
using System;
using System.Collections.Generic;

public partial class FlashcardChallengeTrueOrFalse : Control, IFlashcardChallenge
{
	private Panel _challengePanel;
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

	private const float FeedbackDisplayTime = 4.0f; // Seconds to show correct/incorrect feedback before hiding

    public bool Visibility => Visible;

	public override void _Ready()
	{
		// Get UI elements
		_challengePanel = GetNode<Panel>("ChallengePanel");
		_questionLabel = GetNode<Label>("ChallengePanel/MarginContainer/VBoxContainer/QuestionLabel");
        _answerLabel = GetNode<Label>("ChallengePanel/MarginContainer/VBoxContainer/AnswerLabel");
		_contextLabel = GetNode<Label>("ChallengePanel/MarginContainer/VBoxContainer/ContextLabel");
		_trueButton = GetNode<Button>("ChallengePanel/MarginContainer/VBoxContainer/ButtonContainer/TrueButton");
		_falseButton = GetNode<Button>("ChallengePanel/MarginContainer/VBoxContainer/ButtonContainer/FalseButton");

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

	public void ShowChallenge(Flashcard card, string context = "Answer correctly or bad things happen...")
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

		// Play sound
		if (isCorrect)
		{
			AudioManager.Instance?.PlayCorrectSound();
		}
		else
		{
			AudioManager.Instance?.PlayWrongSound();
		}

		// Visual feedback
		if (isCorrect)
		{
			_trueButton.Modulate = new Color(0.5f, 1.0f, 0.5f);
			_falseButton.Modulate = new Color(0.5f, 1.0f, 0.5f);
		}
		else
		{
			_trueButton.Modulate = new Color(1.0f, 0.5f, 0.5f);
			_falseButton.Modulate = new Color(1.0f, 0.5f, 0.5f);
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
		// Pick a random card and check if its the same
		Flashcard card = FlashcardManager.Instance?.GetRandomCard();

		if (FlashcardManager.Instance.GetActiveCardCount() <= 1)
		{
			// If there's only one card, we can't get a different one, so return card
			return card;
		}

		while (card == excludeCard)
		{
			card = FlashcardManager.Instance?.GetRandomCard();
		}

		// Fall back to the original card if the deck has only one card
		return card;
	}
}
