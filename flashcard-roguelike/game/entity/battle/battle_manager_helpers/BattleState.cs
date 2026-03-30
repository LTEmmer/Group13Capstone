using Godot;
using System.Collections.Generic;

// Tracks all combat state including entities, components, flags, and current battle status.
// This class is a pure data container with no logic
public class BattleState
{
    // Entity references
    public Player Player { get; set; }
    public List<EnemyFSM> AliveEnemies { get; private set; } = new List<EnemyFSM>();
    
    // Component dictionaries
    public AttackComponent PlayerAttack { get; set; }
    public HealthComponent PlayerHealth { get; set; }
    public Dictionary<EnemyFSM, AttackComponent> EnemyAttackComponents { get; private set; } = new Dictionary<EnemyFSM, AttackComponent>();
    public Dictionary<EnemyFSM, HealthComponent> EnemyHealthComponents { get; private set; } = new Dictionary<EnemyFSM, HealthComponent>();
    public Dictionary<EnemyFSM, EnemyStatusComponent> EnemyStatusComponents { get; private set; } = new Dictionary<EnemyFSM, EnemyStatusComponent>();
    
    // Combat flow state
    public bool InCombat { get; set; } = false;
    public bool WaitingForAction { get; set; } = false;
    public bool WaitingForFlashcard { get; set; } = false;
    public string PendingAction { get; set; } = "";
    public int CurrentEnemyIndex { get; set; } = 0;

    // Boss battle state
    public bool IsBossBattle { get; set; } = false;
    public int ConsecutiveCorrect { get; set; } = 0;
    public int BossStreakRequired { get; set; } = 3;
    public float BossBlockReduction { get; set; } = 0.5f;
    
    // Reset all state for a new battle
    public void Reset()
    {
        InCombat = false;
        WaitingForAction = false;
        WaitingForFlashcard = false;
        PendingAction = "";
        CurrentEnemyIndex = 0;
        IsBossBattle = false;
        ConsecutiveCorrect = 0;
        AliveEnemies.Clear();
        EnemyAttackComponents.Clear();
        EnemyHealthComponents.Clear();
        EnemyStatusComponents.Clear();
        Player = null;
        PlayerAttack = null;
        PlayerHealth = null;
    }
    
    // Initialize state with entities
    public void Initialize(Player player, List<EnemyFSM> enemies)
    {
        Player = player;
        AliveEnemies = new List<EnemyFSM>(enemies);
    }
    
    // Remove a defeated enemy from all tracking
    public void RemoveEnemy(EnemyFSM enemy)
    {
        // Remove enemy from alive list and component dictionaries
        AliveEnemies.Remove(enemy);
        EnemyAttackComponents.Remove(enemy);
        EnemyHealthComponents.Remove(enemy);
        
        if (EnemyStatusComponents.ContainsKey(enemy))
        {
            EnemyStatusComponents[enemy].SlideOut(); // Start slide out animation
            EnemyStatusComponents.Remove(enemy);
        }
    }
}
