using Godot;
using System.Collections.Generic;

// Tracks all combat state including entities, components, flags, and current battle status.
// This class is a pure data container with no logic
public class BattleState
{
    // Entity references
    public Player Player { get; set; }
    public List<EnemyExample> AliveEnemies { get; private set; } = new List<EnemyExample>();
    
    // Component dictionaries
    public AttackComponent PlayerAttack { get; set; }
    public HealthComponent PlayerHealth { get; set; }
    public Dictionary<EnemyExample, AttackComponent> EnemyAttackComponents { get; private set; } = new Dictionary<EnemyExample, AttackComponent>();
    public Dictionary<EnemyExample, HealthComponent> EnemyHealthComponents { get; private set; } = new Dictionary<EnemyExample, HealthComponent>();
    public Dictionary<EnemyExample, EnemyStatusComponent> EnemyStatusComponents { get; private set; } = new Dictionary<EnemyExample, EnemyStatusComponent>();
    
    // Combat flow state
    public bool InCombat { get; set; } = false;
    public bool WaitingForAction { get; set; } = false;
    public bool WaitingForFlashcard { get; set; } = false;
    public string PendingAction { get; set; } = "";
    public int CurrentEnemyIndex { get; set; } = 0;
    
    // Reset all state for a new battle
    public void Reset()
    {
        InCombat = false;
        WaitingForAction = false;
        WaitingForFlashcard = false;
        PendingAction = "";
        CurrentEnemyIndex = 0;
        AliveEnemies.Clear();
        EnemyAttackComponents.Clear();
        EnemyHealthComponents.Clear();
        EnemyStatusComponents.Clear();
        Player = null;
        PlayerAttack = null;
        PlayerHealth = null;
    }
    
    // Initialize state with entities
    public void Initialize(Player player, List<EnemyExample> enemies)
    {
        Player = player;
        AliveEnemies = new List<EnemyExample>(enemies);
    }
    
    // Remove a defeated enemy from all tracking
    public void RemoveEnemy(EnemyExample enemy)
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
