using Godot;
using System.Collections.Generic;
using System;

// Coordinates all battle UI updates including health displays, combat log messages,
// and UI visibility states. Acts as the bridge between battle logic and UI components.
public class BattleUICoordinator
{
    private BattleUI _battleUI;
    private BattleState _state;
    
    public void Initialize(BattleUI battleUI, BattleState state)
    {
        _battleUI = battleUI;
        _state = state;
    }
    
    // Log a message to the combat log
    public void LogMessage(string message)
    {
        _battleUI?.AddCombatLog(message);
    }
    
    // Enable or disable action buttons
    public void SetActionsEnabled(bool enabled)
    {
        _battleUI?.SetActionsEnabled(enabled); // Switches them
    }
    
    // Update all health displays
    public void UpdateHealthUI()
    {
        if (_battleUI != null)
        {
            // Update player health display
            _battleUI.UpdatePlayerHealth(_state.PlayerHealth.CurrentHealth, _state.PlayerHealth.MaxHealth);
        }
        
        foreach (var enemy in _state.AliveEnemies)
        {
            // Update each enemy's health display
            var status = _state.EnemyStatusComponents[enemy];
            var health = _state.EnemyHealthComponents[enemy];
            status.SetHealth(health.CurrentHealth, health.MaxHealth);
        }
    }
    
    // Initialize battle UI for combat start
    public void InitializeBattleUI()
    {
        if (_battleUI != null)
        {
            // Clear combat log and show initial message
            _battleUI.ClearCombatLog();
            _battleUI.AddCombatLog("Battle started!");
            _battleUI.SlideIn();
        }
    }
    
    // Slide out all enemy status displays
    public void SlideOutEnemyStatus()
    {
        // Trigger slide out animation for each enemy status display
        foreach (var status in _state.EnemyStatusComponents)
        {
            status.Value.SlideOut();
        }
    }
    
    // Handle battle end UI sequence
    public void HandleBattleEndUI(bool victory, bool ran, Action onComplete = null)
    {
        // If battle UI is not initialized, just call onComplete immediately
        if (_battleUI == null)
        {
            onComplete?.Invoke();
            return;
        }
        
        // Show end message
        if (ran)
        {
            _battleUI.AddCombatLog("Escaped from battle!");
        }
        else if (victory)
        {
            _battleUI.AddCombatLog("Victory! All enemies defeated!");
        }
        else
        {
            _battleUI.AddCombatLog("lol you died L bozo");
        }
        
        SlideOutEnemyStatus();
        
        // Return callback to be invoked after UI animation (if provided)
        onComplete?.Invoke();
    }
    
    // Slide out battle UI with callback
    public void SlideOutBattleUI(System.Action onComplete)
    {
        _battleUI?.SlideOut(onComplete);
    }
}
