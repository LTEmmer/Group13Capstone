using Godot;

// Temporary boss for testing boss combat mechanics.
public partial class TestBoss : EnemyFSM, IBossEnemy
{
    [Export] public int StreakRequired { get; set; } = 3;
    [Export] public float BlockReduction { get; set; } = 0.5f;
}
