using System.IO;
using System.Collections.Generic;
using System.Text.Json;
using Godot;

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
        // Globalize the path and check the save directory for any sets 
        // Create the directory and return an empty list if the directory doesn't exist
        string dir = ProjectSettings.GlobalizePath(SaveDirectory);
        if (!Directory.Exists(dir))
        {
            GD.PrintErr("No flashcard sets found, save directory does not exist. Creating directory: " + dir);
            Directory.CreateDirectory(dir);
            return new List<FlashcardSet>();
        }

        List<FlashcardSet> sets = new();
        int setsFound = 0;

        // For each JSON file, read the file and deserialize it into FlashcardSet objects then return the list of sets
        foreach (string file in Directory.GetFiles(dir, "*.json"))
        {
            string json = File.ReadAllText(file);
            FlashcardSet set = JsonSerializer.Deserialize<FlashcardSet>(json);

            // Check if deserialization was successful, if not print an error and skip the file
            if (set == null)
            {
                GD.PrintErr("Failed to deserialize flashcard set from file: " + file);
                continue;
            }

            // Add the set to the list of sets to return 
            sets.Add(set);
            ++setsFound;
        }

        GD.Print("Loaded " + setsFound + " flashcard sets.");
        return sets;
    }
}