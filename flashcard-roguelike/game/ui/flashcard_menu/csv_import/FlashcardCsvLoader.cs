using CsvHelper;
using CsvHelper.Configuration;
using Godot;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

public sealed class FlashcardCsvLoader
{
    // For now assume at most 2 columns: Question and Answer, can be expanded later
    private readonly int _columnMaximum = 2; 

    public FlashcardSet ImportCsv(string filePath, string setName = null)
    {   
        // Check if the file exists
        if (!File.Exists(filePath))
        {
            GD.PrintErr("File not found at: " + filePath);
            return null;
        }

        // Check for valid headers
        int headerStatus = HasValidHeaders(filePath);
        if (headerStatus == -1)
        {
            GD.PrintErr("Excessive amount of columns in CSV file: " + filePath);
            return null;
        }
        bool validHeaders = headerStatus == 1;

        // "using" ensures automatic disposal of the reader and csv objects
        // "var" is used for readability, at compile time the correct types are inferred
        using var reader = new StreamReader(filePath);
        using var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = validHeaders, // Set based on whether the CSV has valid headers
            TrimOptions = TrimOptions.Trim, // Trim whitespace
            PrepareHeaderForMatch = args => args.Header.Trim().ToLower(), // Make header matching case-insensitive w/o extra whitespace
            MissingFieldFound = null, // Disable missing field validation to allow for blank handling
            BadDataFound = context =>
            {
                GD.PrintErr($"Check for unescaped quotes or invalid characters (like emojis). Bad data found on row : {context.RawRecord}");
            },
        });

        if (validHeaders)
        {
            // Use the mapping defined to map CSV columns to the FlashcardCsvRecord class
            csv.Context.RegisterClassMap<FlashcardMap>(); 
        }
        else
        {
            // If there are no valid headers, map by index assuming the first column is the question and the second is the answer
            csv.Context.RegisterClassMap<HeaderlessFlashcardMap>();
        }
        

        // Read all the records from the CSV file and convert them to FlashcardCsvRecord
        var records = csv.GetRecords<FlashcardCsvRecord>();
        List<Flashcard> cards = new();

        // For each valid record create a Flashcard object and add it to the list
        foreach (var record in records)
        {   
            // Handle blanks in this way for now, skip
            if (string.IsNullOrWhiteSpace(record.Question) || string.IsNullOrWhiteSpace(record.Answer))
            {
                GD.PrintErr("Skipping record with blank question or answer from CSV file: " + filePath + ". Question: " + record.Question + " Answer: " + record.Answer);
                continue;
            }

            // Add the flashcard to the list
            cards.Add(new Flashcard
            {
                Question = record.Question,
                Answer = record.Answer
            });
        }

        // If no valid cards were found, return null to indicate failure
        if (cards.Count == 0)
        {
            GD.PrintErr("No valid flashcards found in CSV file: " + filePath);
            return null;
        }

        // If no set name was provided, use the file name without extension as the set name
        if (string.IsNullOrWhiteSpace(setName))
        {
            setName = Path.GetFileNameWithoutExtension(filePath);
        }

        // Create and return a FlashcardSet with the list of cards and setName as the display name
        return new FlashcardSet
        {
            DisplayName = setName,
            Cards = cards
        };
    }

    private int HasValidHeaders(string path)
    {   
        // -1 indicates not enough columns, 0 indicates no valid headers, 1 indicates valid headers
        // Create a reader to handle headers since CsvHelper doesn't do it out the box easily
        using var tmpReader = new StreamReader(path);
        using var tmpCsv = new CsvReader(tmpReader, new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = false, // Don't treat the first row as headers, we want to check them manually
        });

        // Read the first row, check if empty and clean it for comparison
        if (!tmpCsv.Read())
        {
            return -1; // Empty CSV, not valid
        }

        string[] firstRow = tmpCsv.Parser.Record;
        if (firstRow.Length > _columnMaximum)
        {
            return -1; // Not enough columns, not valid
        }

        List<string> cleanedFirstRow = new();
        foreach (string h in firstRow)
        {
            string cleaned = h.Trim().ToLower();
            cleanedFirstRow.Add(cleaned);
        }

        // Check if any of the headers in the first row match the expected question and answer headers, both must be present to be valid
        bool hasQuestionHeader = cleanedFirstRow.Any(h => FlashcardMap.ValidQuestionHeaders.Contains(h));
        bool hasAnswerHeader = cleanedFirstRow.Any(h => FlashcardMap.ValidAnswerHeaders.Contains(h));

        return hasQuestionHeader && hasAnswerHeader ? 1 : 0;
    }
}