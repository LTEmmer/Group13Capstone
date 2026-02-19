using CsvHelper.Configuration;

/* FlashcardCsvRecord is a class used to ensure CSV columns are mapped properly.
Can be expanded later to account for more complex CSV structures, for now just a simple question and answer.
If expanded, ensure to update the FlashcardMap and HeaderlessFlashcardMap to properly map the new properties to the correct columns. */
public sealed class FlashcardCsvRecord
{
    public string Question { get; set; }
    public string Answer { get; set; }
}

// Maps column names in the CSV to properties on FlashcardCsvRecord, can be expanded to support more column name variations if needed
public sealed class FlashcardMap : ClassMap<FlashcardCsvRecord>
{
    public static readonly string[] ValidQuestionHeaders = { "question", "front", "q", "term" };
    public static readonly string[] ValidAnswerHeaders = { "answer", "back", "a", "definition" };

    public FlashcardMap()
    {
        Map(m => m.Question).Name(ValidQuestionHeaders); // Map the Question property to any of these question headers
        Map(m => m.Answer).Name(ValidAnswerHeaders); // Map the Answer property to any of these answer headers
    }
}

// Maps columns by index in case of no headers, assume the first col is the question by default
public sealed class HeaderlessFlashcardMap : ClassMap<FlashcardCsvRecord>
{
    public HeaderlessFlashcardMap()
    {
        Map(m => m.Question).Index(0); // Map the Question property to the first column
        Map(m => m.Answer).Index(1); // Map the Answer property to the second column
    }
}