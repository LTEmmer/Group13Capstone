using CsvHelper;
using CsvHelper.Configuration;
using Godot;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

public sealed class FlashcardCsvLoader
{
    public FlashcardSet ImportCsv(string filePath)
    {   
        // Check if the file exists
        if (!File.Exists(filePath))
        {
            GD.PrintErr("File not found at: " + filePath);
            return null;
        }

        // "using" ensures automatic disposal of the reader and csv objects
        // "var" is used for readability, at compile time the correct types are inferred
        using var reader = new StreamReader(filePath);
        using var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = true, // Assume the first row contain headers
            TrimOptions = TrimOptions.Trim, // Trim whitespace
            
        });

        // Use the mapping defined to map CSV columns to the FlashcardCsvRecord class
        csv.Context.RegisterClassMap<FlashcardMap>(); 

        // Read all the records from the CSV file and convert them to FlashcardCsvRecord
        var records = csv.GetRecords<FlashcardCsvRecord>();
        List<Flashcard> cards = new();

        // For each valid record create a Flashcard object and add it to the list
        foreach (var record in records)
        {   
            // Skip blanks for now
            if (string.IsNullOrWhiteSpace(record.Question) || string.IsNullOrWhiteSpace(record.Answer))
                continue;
            
            // Add the flashcard to the list trimming any extra whitespace
            cards.Add(new Flashcard
            {
                Question = record.Question,
                Answer = record.Answer
            });
        }

        // Create and return a FlashcardSet with the list of cards named after the CSV
        return new FlashcardSet
        {
            DisplayName = Path.GetFileNameWithoutExtension(filePath),
            Cards = cards
        };
    }
}