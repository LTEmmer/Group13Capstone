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
		bool showTrue = new Random().Next(2) == 0;
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
			_contextLabel.Text = "Incorrect!";
		}

		// Wait a moment then hide and emit result
		GetTree().CreateTimer(4.0f).Timeout += () =>
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

	public Flashcard LoadRandomCard()
	{
		List<FlashcardSet> sets = FlashcardManager.Instance.ActiveFlashCardLists;

		if (sets == null || sets.Count == 0)
		{
			GD.PrintErr("FlashcardChallengeTrueOrFalse: No flashcard sets available.");
			return null;
		}

        // Get a random card from all active sets
		List<Flashcard> allCards = new List<Flashcard>();
		foreach (var set in sets)
		{
			if (set.Cards != null)
				allCards.AddRange(set.Cards);
		}

		if (allCards.Count == 0)
		{
			GD.PrintErr("FlashcardChallengeTrueOrFalse: No flashcards available in sets.");
			return null;
		}

		Random random = new Random();
		return allCards[random.Next(allCards.Count)];
	}

	private Flashcard GetRandomDifferentCard(Flashcard excludeCard)
	{
		List<FlashcardSet> sets = FlashcardManager.Instance.ActiveFlashCardLists;

		if (sets == null || sets.Count == 0) return excludeCard;

        // Get a list of all other cards
		List<Flashcard> otherCards = new List<Flashcard>();
		foreach (var set in sets)
		{
			if (set.Cards != null)
			
            foreach (var card in set.Cards)
            {
                if (card == excludeCard) continue; // Skip the card we want to exclude
                otherCards.Add(card);
            } 
		}

		if (otherCards.Count == 0) return excludeCard;

        // Return a random other card
		Random random = new Random();
		return otherCards[random.Next(otherCards.Count)];
	}
}
