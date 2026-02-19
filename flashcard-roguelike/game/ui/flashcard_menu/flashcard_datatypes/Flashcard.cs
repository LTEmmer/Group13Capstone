/* Basic representation of a flashcard with a question and answer.
Can be expanded later to include things like difficulty, hints, etc. if desired.
If expanded, ensure to update the FlashcardCsvRecord and mapping in FlashcardMap and 
HeaderlessFlashcardMap to properly map the new properties to the correct columns. */
public sealed class Flashcard
{
	public string Question { get; set; }
	public string Answer { get; set; }
}
