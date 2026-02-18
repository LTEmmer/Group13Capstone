using CsvHelper.Configuration;

// FlashcardCsvRecord is a class used to ensure CSV columns are mapped properly
public sealed class FlashcardCsvRecord
{
    public string Question { get; set; }
    public string Answer { get; set; }
}

// Maps column names in the CSV to properties on FlashcardCsvRecord, can be expanded to support more column name variations if needed
public sealed class FlashcardMap : ClassMap<FlashcardCsvRecord>
{
    public FlashcardMap()
    {
        Map(m => m.Question).Name("question", "front", "q", "term"); // Map the Question property to any of these question headers
        Map(m => m.Answer).Name("answer", "back", "a", "definition"); // Map the Answer property to any of these answer headers
    }
}

// Maps columns by index in case of no headers, assume the first col is the question
public sealed class HeaderlessFlashcardMap : ClassMap<FlashcardCsvRecord>
{
    public HeaderlessFlashcardMap()
    {
        Map(m => m.Question).Index(0); // Map the Question property to the first column
        Map(m => m.Answer).Index(1); // Map the Answer property to the second column
    }
}