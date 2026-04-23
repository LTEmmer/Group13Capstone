using Godot;
using System;
using System.Collections.Generic;

public partial class FlashcardChallengeMultipleChoice : Control, IFlashcardChallenge
{
	private Control _challengePanel;
	private Label _questionLabel;
	private Button _optionAButton;
	private Button _optionBButton;
	private Button _optionCButton;
	private Button _optionDButton;
	private Label _contextLabel;

	private Flashcard _currentCard;
	private Button[] _optionButtons;
	private string _correctAnswer;
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
		_contextLabel = GetNode<Label>("CenterContainer/ChallengePanel/MarginContainer/VBoxContainer/ContextLabel");
		_optionAButton = GetNode<Button>("CenterContainer/ChallengePanel/MarginContainer/VBoxContainer/OptionsContainer/OptionAButton");
		_optionBButton = GetNode<Button>("CenterContainer/ChallengePanel/MarginContainer/VBoxContainer/OptionsContainer/OptionBButton");
		_optionCButton = GetNode<Button>("CenterContainer/ChallengePanel/MarginContainer/VBoxContainer/OptionsContainer/OptionCButton");
		_optionDButton = GetNode<Button>("CenterContainer/ChallengePanel/MarginContainer/VBoxContainer/OptionsContainer/OptionDButton");

		_optionButtons = [_optionAButton, _optionBButton, _optionCButton, _optionDButton];

		// Mouse-only choices: avoid focus on first open so the first click is not consumed by focus/UI.
		foreach (var b in _optionButtons)
		{
			b.FocusMode = FocusModeEnum.None;
		}

		// Connect signals
		_optionAButton.Pressed += () => OnOptionSelected(_optionAButton.Text);
		_optionBButton.Pressed += () => OnOptionSelected(_optionBButton.Text);
		_optionCButton.Pressed += () => OnOptionSelected(_optionCButton.Text);
		_optionDButton.Pressed += () => OnOptionSelected(_optionDButton.Text);

		AudioManager.Instance?.RegisterButton(_optionAButton);
		AudioManager.Instance?.RegisterButton(_optionBButton);
		AudioManager.Instance?.RegisterButton(_optionCButton);
		AudioManager.Instance?.RegisterButton(_optionDButton);

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
		_correctAnswer = card.Answer;
		_isActive = true;

		// Get three incorrect answers from other cards
		List<string> allOptions = GetWrongAnswers(card, 3);

		// Create a list of all options (1 correct + 3 wrong)
		allOptions.Add(_correctAnswer);

		// Shuffle the options
		ShuffleList(allOptions);

		// Assign options to buttons
		for (int i = 0; i < _optionButtons.Length; i++)
		{
			_optionButtons[i].Text = allOptions[i];
			_optionButtons[i].Disabled = false;
		}

		_questionLabel.Text = card.Question;
		_contextLabel.Text = context;
		if (combat)
		{
			_contextLabel.AddThemeColorOverride("font_color", Colors.Red);
		}
		else
		{
			_contextLabel.AddThemeColorOverride("font_color", Colors.Green);
		}

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
		foreach (var button in _optionButtons)
		{
			button.Disabled = true;
		}

		// Hide with fade-out animation
		Tween tween = CreateTween();
		tween.TweenProperty(_challengePanel, "modulate:a", 0.0f, _hideDuration)
			 .SetTrans(Tween.TransitionType.Quad)
			 .SetEase(Tween.EaseType.In);

		tween.TweenCallback(Callable.From(() =>
		{
			Visible = false;
			_currentCard = null;
			onComplete?.Invoke();
		}));
	}

	private void OnOptionSelected(string selectedAnswer)
	{
		if (!_isActive || _currentCard == null) return;

		// Disable all buttons
		foreach (var button in _optionButtons)
		{
			button.Disabled = true;
		}

		// Check if the answer is correct
		bool isCorrect = selectedAnswer.Equals(_correctAnswer, StringComparison.OrdinalIgnoreCase);

		// Play sound
		if (isCorrect)
		{
			AudioManager.Instance?.PlayCorrectSound();
		}
		else
		{
			AudioManager.Instance?.PlayWrongSound();
		}

		// Visual feedback - highlight all buttons
		Color correctColor = new(0.5f, 1.0f, 0.5f); // Green for correct
		Color wrongColor = new(1.0f, 0.5f, 0.5f);   // Red for wrong
		Color neutralColor = Colors.White;

		foreach (var button in _optionButtons)
		{
			if (button.Text.Equals(_correctAnswer, StringComparison.OrdinalIgnoreCase))
			{
				button.Modulate = correctColor; // Highlight the correct answer
			}
			else if (!isCorrect && button.Text.Equals(selectedAnswer, StringComparison.OrdinalIgnoreCase))
			{
				button.Modulate = wrongColor; // Highlight the selected wrong answer
			}
			else
			{
				button.Modulate = neutralColor; // Keep unselected wrong answers neutral
			}
		}

		if (!isCorrect)
		{
			_contextLabel.Text = $"Incorrect! The correct answer is: {_correctAnswer}";
		}

		// Wait a moment then hide and emit result
		GetTree().CreateTimer(FeedbackDisplayTime).Timeout += () =>
		{
			foreach (var button in _optionButtons)
			{
				button.Modulate = Colors.White;
			}
			HideChallenge(() => _onAnswerSubmitted?.Invoke(isCorrect));
		};
	}

	private static List<string> GetWrongAnswers(Flashcard excludeCard, int count)
	{
		List<FlashcardSet> sets = FlashcardManager.Instance.ActiveFlashCardLists;
		List<string> wrongAnswers = [];

		if (sets == null || FlashcardManager.Instance.GetActiveCardCount() <= count) 
		{
			return wrongAnswers; // Not enough cards to get wrong answers, return empty list
		}

		// Get all cards excluding the current one
		List<Flashcard> otherCards = FlashcardManager.Instance.GetActiveCards();
		otherCards.Remove(excludeCard);

		// Get up to 'count' wrong answers
		for (int i = 0; i < count && otherCards.Count > 0; i++)
		{
			int randomIndex = _rng.Next(otherCards.Count);
			wrongAnswers.Add(otherCards[randomIndex].Answer);
			otherCards.RemoveAt(randomIndex);
		}

		// If we don't have enough wrong answers, add filler (shouldn't happen with real data)
		while (wrongAnswers.Count < count && otherCards.Count > 0)
		{
			wrongAnswers.Add("No other cards available");
		}

		return wrongAnswers;
	}

	private static void ShuffleList(List<string> list)
	{
		// Basic shuffling
		for (int i = list.Count - 1; i > 0; i--)
		{
			int randomIndex = _rng.Next(i + 1);
			(list[i], list[randomIndex]) = (list[randomIndex], list[i]);
		}
	}
}
