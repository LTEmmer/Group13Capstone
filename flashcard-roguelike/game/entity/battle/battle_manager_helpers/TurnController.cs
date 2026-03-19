using Godot;
using System;

// Manages the flow of turns in combat: player turn → enemy turns → player turn.
public class TurnController
{
    private BattleState _state;
    private BattleUICoordinator _uiCoordinator;
    private CombatResolver _combatResolver;
    
    public void Initialize(BattleState state, BattleUICoordinator uiCoordinator, CombatResolver combatResolver)
    {
        _state = state;
        _uiCoordinator = uiCoordinator;
        _combatResolver = combatResolver;
    }
    
    // Start the player's turn
    public void StartPlayerTurn()
    {
        if (!_state.InCombat) return; // Safety check
        
        // Set state to wait for player action and enable action buttons
        _state.WaitingForAction = true;
        _uiCoordinator.LogMessage("Your turn!");
        _uiCoordinator.SetActionsEnabled(true);
    }
    
    // Start the enemy turn sequence
    public void StartEnemyTurns()
    {
        // Set state to not wait for player action and disable action buttons
        _state.CurrentEnemyIndex = 0;
        ExecuteNextEnemyTurn();
    }
    
    // Execute the next enemy's turn
    public void ExecuteNextEnemyTurn()
    {
        if (!_state.InCombat) return; // Safety check: battle may have ended (e.g. player died)

        if (_state.CurrentEnemyIndex >= _state.AliveEnemies.Count)
        {
            // All enemies have taken their turn
            StartPlayerTurn();
            return;
        }
        
        // Get an enemy reference for the current turn and emit event to notify UI and other systems
        // Emit event to notify UI and other systems that an enemy turn is executing
        var enemy = _state.AliveEnemies[_state.CurrentEnemyIndex];
        _uiCoordinator.LogMessage($"{enemy.Name} attacks!");
        
        // Trigger defense challenge for the player
        _combatResolver.StartDefenseChallenge(_state.CurrentEnemyIndex);
    }
    
    // Move to the next enemy turn after a delay
    public void AdvanceToNextEnemy(SceneTree tree)
    {
        _state.CurrentEnemyIndex++;
        tree.CreateTimer(0.5).Timeout += ExecuteNextEnemyTurn;
    }
}
