using Godot;
using System;

public partial class ShootingEventRoom : Room, IEventRoom
{
    [Export] public BaseNPC TriggerNPC;
    public bool IsCompleted { get; private set; }
    public float Difficulty { get; private set; }

    public override void _Ready()
    {
        base._Ready();
        Difficulty = GameDifficultyManager.Instance.getCurrentDifficultyScore();

        TriggerNPC.OnInteraction += TriggerEvent;
    }

    public void TriggerEvent()
    {
        // For testing, just complete the event immediately with success.
        GD.Print("Shooting event triggered! Starting challenge...");
        CompleteEvent(true);

        // Later move to turret to shoot
    }

    public void CompleteEvent(bool success)
    {
        if (IsCompleted) return; // Prevent multiple completions
        IsCompleted = true;

        if (success)
        {
            ApplyReward();
        }
        else
        {
            ApplyPenalty();
        }

        EventManager.Instance.raise("on_room_clear", "test"); // Tell connections to open
    }

    public void ApplyReward()
    {
        GD.Print("Player has completed the shooting event room! Reward applied.");
    }

    public void ApplyPenalty()
    {
        GD.Print("Player has failed the shooting event room! Penalty applied.");
    }


}
