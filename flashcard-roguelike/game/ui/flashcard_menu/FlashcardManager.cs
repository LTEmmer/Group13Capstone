using Godot;
using System.Collections.Generic;

public partial class FlashcardManager : Node
{
    // Singleton of FlashcardManager for access from anywhere, set in _Ready
    public static FlashcardManager Instance { get; private set; }

    // _persistence is used to load and save flashcard sets, _availableSets is the list of all sets in the user's save directory
    private FlashcardPersistence _persistence = new();
    private List<FlashcardSet> _availableSets = new();

    public override void _Ready()
    {
        Instance = this; // Set the singleton
        _availableSets = _persistence.LoadAllSets(); // Load all sets from the "user://" directory
    }

    // Expose the available sets as a read-only list for other code to access, other code should not modify the list
    public IReadOnlyList<FlashcardSet> AvailableSets => _availableSets;

    // Singleton method to handle importing and saving flashcards from CSV files and add them to the available sets, input the path and name
    public void ImportAndSave(string csvPath, string setName = null)
    {
        FlashcardSet set = new FlashcardCsvLoader().ImportCsv(csvPath, setName);

        _persistence.SaveSet(set);
        _availableSets.Add(set);
    }
}