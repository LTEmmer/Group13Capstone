using Godot;
using System;
using System.Collections.Generic;

public partial class FlashcardManager : Node
{
	// Singleton of FlashcardManager for access from anywhere
	public static FlashcardManager Instance { get; private set; }
	private FlashcardPersistence _persistence = new();
	private List<FlashcardSet> AvailableSets = new();

	// Cached flat list of all cards across sets rebuilt only when sets change
	private List<Flashcard> _cardCache = null;
	private readonly Random _random = new();

	// Shuffled deck for cycle-through drawing, every card is dealt once before reshuffling
	private List<Flashcard> _shuffledDeck = null;
	private int _deckIndex = 0;

	public List<FlashcardSet> ActiveFlashCardLists
	{
		get {
			if (Instance == null)
			{
				return new List<FlashcardSet>();
			}
			return Instance.AvailableSets;
		} set
		{
			if (value == null) return;
			Instance.AvailableSets = value;
		}
	}

	public override void _Ready()
	{
		Instance = this;
		AvailableSets = _persistence.LoadAllSets();

		/* For demonstration, run this scene on its own and close it, there are no visuals.
		It should import and save these three sets from CSV on startup.
		In practice this would be triggered by user input. Only the two labeled as valid
		should be saved successfully to available sets and to: user://flashcards/
		The example CSV files are located in example_csv.
		REMOVE when actual implementation for adding from menu is ready. */
		//ImportAndSave(ProjectSettings.GlobalizePath("res://game/ui/flashcard_menu/example_csv/LabeledHeaders_Valid.csv"), "LabeledTest");
		//ImportAndSave(ProjectSettings.GlobalizePath("res://game/ui/flashcard_menu/example_csv/UnlabeledHeaders_Valid.csv"), "UnlabeledTest");
		//ImportAndSave(ProjectSettings.GlobalizePath("res://game/ui/flashcard_menu/example_csv/TooManyColumns_Invalid.csv"), "FailureTest");
		ImportAndSave(ProjectSettings.GlobalizePath("res://game/ui/flashcard_menu/example_csv/SampleCards.csv"));
	}

	// Singleton method to handle importing and saving flashcards from CSV files and add them to the available sets
	public void ImportAndSave(string csvPath, string setName = null)
	{
		FlashcardSet set = new FlashcardCsvLoader().ImportCsv(csvPath, setName);

		if (set != null && !AvailableSets.Exists(s => s.DisplayName == set.DisplayName))
		{
			_persistence.SaveSet(set);
			AvailableSets.Add(set);
			_cardCache = null; // Invalidate cache so GetRandomCard() rebuilds on next call
		}
	}

	public void CreateAndSaveSet(string displayName, List<Flashcard> cards)
	{
		if (string.IsNullOrWhiteSpace(displayName) || cards == null || cards.Count == 0)
		{
			GD.PushError("CreateAndSaveSet: displayName is empty or cards list is null/empty.");
			return;
		}
		
		string trimmed = displayName.Trim();
		if (AvailableSets.Exists(s => s.DisplayName == trimmed))
		{
			GD.PushError($"CreateAndSaveSet: a set named '{trimmed}' already exists.");
			return;
		}
		var set = new FlashcardSet { DisplayName = trimmed, Cards = cards };
		_persistence.SaveSet(set);
		AvailableSets.Add(set);
		_cardCache = null;
	}

	public void SetActive(string displayName, bool active)
	{
		FlashcardSet set = AvailableSets.Find(s => s.DisplayName == displayName);
		if (set == null) 
		{
			return;
		}

		set.IsActive = active;
		_persistence.SaveSet(set);
		_cardCache = null;
	}

	private void BuildCardCache()
	{
		_cardCache = new();
		foreach (var set in AvailableSets)
		{
			if (set.IsActive && set.Cards != null)
				_cardCache.AddRange(set.Cards);
		}

		_shuffledDeck = new List<Flashcard>(_cardCache);
		ShuffleDeck();
	}

	private void ShuffleDeck()
	{
		for (int i = _shuffledDeck.Count - 1; i > 0; i--)
		{
			int j = _random.Next(i + 1);
			(_shuffledDeck[i], _shuffledDeck[j]) = (_shuffledDeck[j], _shuffledDeck[i]);
		}
		_deckIndex = 0;
	}

	// Returns a flashcard drawn from a shuffled deck. Every card is dealt once before reshuffling.
	public Flashcard GetRandomCard()
	{
		if (_cardCache == null)
			BuildCardCache();

		if (_shuffledDeck.Count == 0)
		{
			GD.PrintErr("FlashcardManager: No flashcards available in active sets.");
			return null;
		}

		if (_deckIndex >= _shuffledDeck.Count)
			ShuffleDeck();

		return _shuffledDeck[_deckIndex++];
	}

	public List<Flashcard> GetActiveCards()
	{
		if (_cardCache == null)
		{
			BuildCardCache();
		}
		
		return new(_cardCache);
	}

	public int GetActiveCardCount()
	{
		return _cardCache.Count;
	}

	public bool HasActiveSet()
	{
		return AvailableSets.Exists(s => s.IsActive && s.Cards != null && s.Cards.Count > 0);
	}

	// Delete a flashcard set from available sets and from the file system
	public bool DeleteSet(string setDisplayName)
	{
		// Find the set in the available sets list
		FlashcardSet setToRemove = AvailableSets.Find(set => set.DisplayName == setDisplayName);

		if (setToRemove != null)
		{
			// Delete the set from the file system
			if (_persistence.DeleteSet(setDisplayName))
			{
				// Remove from available sets list only if deletion was successful
				AvailableSets.Remove(setToRemove);
				_cardCache = null; // Invalidate cache so GetRandomCard() rebuilds on next call
				return true;
			}
		}
		else
		{
			GD.PrintErr($"Flashcard set '{setDisplayName}' not found in available sets");
		}

		return false;
	}
}
