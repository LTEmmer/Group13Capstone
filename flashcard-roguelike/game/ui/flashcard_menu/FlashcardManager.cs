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
	}

	// Returns a random flashcard drawn from all active sets.
	// Uses a cached flat card list rebuilt only when sets are added or removed.
	public Flashcard GetRandomCard()
	{
		// Build cache if missing or invalidated
		if (_cardCache == null)
		{
			BuildCardCache();
		}

		if (_cardCache.Count == 0)
		{
			GD.PrintErr("FlashcardManager: No flashcards available in active sets.");
			return null;
		}

		return _cardCache[_random.Next(_cardCache.Count)];
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
