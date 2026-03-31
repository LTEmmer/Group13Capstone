using Godot;

public interface IEventRoom
{
    void TriggerEvent();

    void CompleteEvent(bool success);

    void ApplyReward();

    void ApplyPenalty();

    bool IsCompleted { get; }

    float Difficulty { get; }
}