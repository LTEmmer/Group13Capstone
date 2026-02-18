using System.Collections.Generic;

// Basic representation of a flashcard set with a name
public sealed class FlashcardSet
{
    public string DisplayName { get; set; }
    public List<Flashcard> Cards { get; set; }
}