using Godot;
using System;

// Interface for all flashcard challenge types to ensure consistency
public interface IFlashcardChallenge
{
    // Signal when answer is submitted with result (true/false for correct/incorrect)
    void ConnectAnswerSubmitted(Action<bool> callback);
    
    // Show the challenge with a flashcard and optional context
    void ShowChallenge(Flashcard card, string context = "Answer correctly or bad things happen...");
    
    // Hide the challenge with optional completion callback
    void HideChallenge(Action onComplete = null);
    
    // Load a random flashcard from available sets
    Flashcard LoadRandomCard();
    
    // Get the visibility state
    bool Visibility { get; }
}
