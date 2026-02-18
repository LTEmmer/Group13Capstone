using Godot;
using System.Collections.Generic;

public partial class FlashcardManager : Node
{
    // Singleton of FlashcardManager for access from anywhere
    public static FlashcardManager Instance { get; private set; }
    private FlashcardPersistence _persistence = new();
    public List<FlashcardSet> AvailableSets = new();

    public override void _Ready()
    {
        Instance = this;
        AvailableSets = _persistence.LoadAllSets();
    }

    // Singleton method to handle importing and saving flashcards from CSV files and add them to the available sets
    public void ImportAndSave(string csvPath, string setName = null)
    {
        FlashcardSet set = new FlashcardCsvLoader().ImportCsv(csvPath, setName);

        _persistence.SaveSet(set);
        AvailableSets.Add(set);
    }
}