using CsvHelper.Configuration;

// FlashcardCsvRecord is a class used to ensure CSV columns are mapped properly
public sealed class FlashcardCsvRecord
{
    public string Question { get; set; }
    public string Answer { get; set; }
}

// Maps column names in the CSV to properties on FlashcardCsvRecord
public sealed class FlashcardMap : ClassMap<FlashcardCsvRecord>
{
    public FlashcardMap()
    {
        Map(m => m.Question).Name("Question");
        Map(m => m.Answer).Name("Answer");
    }
}