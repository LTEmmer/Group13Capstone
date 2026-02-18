using System.IO;
using System.Collections.Generic;
using System.Text.Json;
using Godot;
using System.Dynamic;

public sealed class FlashcardPersistence
{
    // Note: Godot has a special "user://" path that points to a writable directory for the game, use this to save flashcard sets
    private const string SaveDirectory = "user://flashcards/";

    // Save a set of flashcards to its own .json file to the save directory
    public void SaveSet(FlashcardSet set)
    {
        // GlobalizePath converts the "user://" path to an actual file system path, then ensure the directory exists
        string dir = ProjectSettings.GlobalizePath(SaveDirectory);
        Directory.CreateDirectory(dir);

        // Create the full path for the set using its DisplayName and serialize the FlashcardSet to JSON, then write it to a file
        string path = Path.Combine(dir, $"{set.DisplayName}.json");
        string json = JsonSerializer.Serialize(set, new JsonSerializerOptions
        {
            WriteIndented = true
        });

        File.WriteAllText(path, json);
    }

    public List<FlashcardSet> LoadAllSets()
    {
        // Globalize the path and check the save directory for any sets, return an empty list if the directory doesn't exist
        string dir = ProjectSettings.GlobalizePath(SaveDirectory);
        if (!Directory.Exists(dir))
            return new List<FlashcardSet>();

        List<FlashcardSet> sets = new();

        // For each JSON file, read the file and deserialize it into FlashcardSet objects then return the list of sets
        foreach (string file in Directory.GetFiles(dir, "*.json"))
        {
            string json = File.ReadAllText(file);
            FlashcardSet set = JsonSerializer.Deserialize<FlashcardSet>(json);
            sets.Add(set);
        }

        return sets;
    }
}