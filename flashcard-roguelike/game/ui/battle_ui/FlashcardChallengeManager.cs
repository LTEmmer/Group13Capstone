using Godot;
using System;

// Manages all flashcard challenge types and selects which type to show based on difficulty
// Handles visibility and interaction with all three challenge implementations
public class FlashcardChallengeManager
{
	private FlashcardChallenge _textChallenge;
	private FlashcardChallengeTrueOrFalse _trueOrFalseChallenge;
	private FlashcardChallengeMultipleChoice _multipleChoiceChallenge;

	private IFlashcardChallenge _currentChallenge;
	private Action<bool> _onAnswerSubmittedCallback;

	public void Initialize(
		FlashcardChallenge textChallenge,
		FlashcardChallengeTrueOrFalse trueOrFalseChallenge,
		FlashcardChallengeMultipleChoice multipleChoiceChallenge)
	{
		_textChallenge = textChallenge;
		_trueOrFalseChallenge = trueOrFalseChallenge;
		_multipleChoiceChallenge = multipleChoiceChallenge;

		// Connect answer submitted callbacks from all challenges
		_textChallenge.ConnectAnswerSubmitted(OnAnswerSubmitted);
		_trueOrFalseChallenge.ConnectAnswerSubmitted(OnAnswerSubmitted);
		_multipleChoiceChallenge.ConnectAnswerSubmitted(OnAnswerSubmitted);
	}

	public void SetAnswerSubmittedCallback(Action<bool> callback)
	{
		_onAnswerSubmittedCallback = callback;
	}

	// Show a challenge with difficulty parameter (default 0 = T/F, 1 = MC, 2 = Text)
	public void ShowChallenge(Flashcard card, string context = "Answer correctly or bad things happen...", float difficulty = 0, bool combat = true)
	{
		if (card == null) return;

		// Select challenge type based on difficulty
		IFlashcardChallenge challengeToShow = SelectChallengeByDifficulty(difficulty);

		// Hide the current challenge if one is active
		if (_currentChallenge != null && _currentChallenge.Visibility)
		{
			_currentChallenge.HideChallenge();
		}

		// Show the selected challenge
		_currentChallenge = challengeToShow;
		_currentChallenge.ShowChallenge(card, context, combat);
	}

	// Hide the current active challenge
	public void HideChallenge(Action onComplete = null)
	{
		if (_currentChallenge != null)
		{
			_currentChallenge.HideChallenge(onComplete);
		}
		else
		{
			onComplete?.Invoke();
		}
	}

	// Load a random flashcard, delegates to FlashcardManager which owns the card data
	public Flashcard LoadRandomCard()
	{
		return FlashcardManager.Instance?.GetRandomCard();
	}

	private IFlashcardChallenge SelectChallengeByDifficulty(float difficulty)
	{
        Random random = new Random();
        float roll = (float)random.NextDouble();

        // Normalize difficulty (1–5 -> 0–1)
        float d = (difficulty - 1) / 4f; // Assuming difficulty ranges from 1 to 5

        float trueFalseChance = 0.7f - (0.6f * d); // Decreases from 70% to 10%
        float multipleChoiceChance = 0.2f + (0.2f * d); // Increases from 20% to 40%
        float textChance = 0.1f + (0.4f * d); // Increases from 10% to 50%

        GD.Print($"Difficulty: {difficulty}, Roll: {roll}, T/F Chance: {trueFalseChance}, MC Chance: {multipleChoiceChance}, Text Chance: {textChance}");

        if (roll < trueFalseChance)
        {
            return _trueOrFalseChallenge;
        }
        else if (roll < trueFalseChance + multipleChoiceChance)
        {
            return _multipleChoiceChallenge;
        }
        else
        {
            return _textChallenge;
        }
	}

	private void OnAnswerSubmitted(bool isCorrect)
	{
		_onAnswerSubmittedCallback?.Invoke(isCorrect);
	}
}
