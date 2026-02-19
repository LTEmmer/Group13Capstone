using Godot;
using System.Collections.Generic;

public partial class FlashcardManager : Node
{
    // Singleton of FlashcardManager for access from anywhere
    public static FlashcardManager Instance { get; private set; }
    private FlashcardPersistence _persistence = new();
    private List<FlashcardSet> AvailableSets = new();

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
        ImportAndSave(ProjectSettings.GlobalizePath("res://game/ui/flashcard_menu/example_csv/LabeledHeaders_Valid.csv"), "LabeledTest");
        ImportAndSave(ProjectSettings.GlobalizePath("res://game/ui/flashcard_menu/example_csv/UnlabeledHeaders_Valid.csv"), "UnlabeledTest");
        ImportAndSave(ProjectSettings.GlobalizePath("res://game/ui/flashcard_menu/example_csv/TooManyColumns_Invalid.csv"), "FailureTest");
    }

    // Singleton method to handle importing and saving flashcards from CSV files and add them to the available sets
    public void ImportAndSave(string csvPath, string setName = null)
    {
        FlashcardSet set = new FlashcardCsvLoader().ImportCsv(csvPath, setName);

        if (set != null)
        {
            _persistence.SaveSet(set);
            AvailableSets.Add(set);
        }
    }    

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
}