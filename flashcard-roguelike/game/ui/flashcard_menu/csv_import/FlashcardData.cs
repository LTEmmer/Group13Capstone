using System.Collections.Generic;

public sealed class Flashcard
{
    public string Question { get; set; }
    public string Answer { get; set; }
}

public sealed class FlashcardSet
{
    public string DisplayName { get; set; }
    public List<Flashcard> Cards { get; set; }
}