using Godot;
using System;
using System.Linq;

// Resolves combat actions including attacks, defenses, running, and items.
// Integrates flashcard challenges with combat outcomes.
public class CombatResolver
{
    private BattleState _state;
    private BattleUICoordinator _uiCoordinator;
    private FlashcardChallengeManager _flashcardChallengeManager;
    private TurnController _turnController;
    
    public event Action<bool> OnBattleEnded; // bool: victory
    public event Action<bool> OnBattleEndedWithRun; // bool: successfully ran
    
    public void Initialize(BattleState state, BattleUICoordinator uiCoordinator, 
        FlashcardChallengeManager flashcardChallengeManager, TurnController turnController)
    {
        _state = state;
        _uiCoordinator = uiCoordinator;
        _flashcardChallengeManager = flashcardChallengeManager;
        _turnController = turnController;
    }
    
    // Start attack sequence with flashcard challenge
    public void StartAttackSequence()
    {
        float difficulty = GameDifficultyManager.Instance.getCurrentDifficultyScore();
        if (_flashcardChallengeManager != null)
        {
            // Load a random flashcard for the attack challenge
            Flashcard card = _flashcardChallengeManager.LoadRandomCard();
            if (card != null)
            {
                // Set state to wait for flashcard answer and show challenge with difficulty
                _state.WaitingForFlashcard = true;
                _flashcardChallengeManager.ShowChallenge(card, "Answer correctly to attack!", difficulty);
            }
            else
            {
                // If no flashcards are available, log an error and execute attack as if it succeeded, shouldnt happen
                GD.PrintErr("CombatResolver: No flashcards available for attack challenge.");
                ExecutePlayerAttack(true);
            }
        }
        else
        {
            // If flashcard challenge manager is not assigned, log an error and execute attack as if it succeeded, shouldnt happen
            GD.PrintErr("CombatResolver: FlashcardChallengeManager is not assigned. Skipping attack challenge.");
            ExecutePlayerAttack(true);
        }
    }
    
    // Start defense sequence with flashcard challenge
    public void StartDefenseChallenge(int enemyIndex)
    {
        _state.PendingAction = "defend";
        
        if (_flashcardChallengeManager != null)
        {
            // Load a random flashcard for the defense challenge
            Flashcard card = _flashcardChallengeManager.LoadRandomCard();
            if (card != null)
            {
                // Set state to wait for flashcard answer and show challenge with default difficulty (0 = text)
                _state.WaitingForFlashcard = true;
                // Use the current game difficulty so defense challenges scale like attacks
                _flashcardChallengeManager.ShowChallenge(card, "Answer correctly to defend!", GameDifficultyManager.Instance.getCurrentDifficultyScore());
            }
            else
            {
                // If no flashcards are available, log an error and execute attack as if defense failed, shouldnt happen
                GD.PrintErr("CombatResolver: No flashcards available for defense challenge.");
                ExecuteEnemyAttack(false);
            }
        }
        else
        {
            // If flashcard challenge manager is not assigned, log an error and execute attack as if defense failed, shouldnt happen
            GD.PrintErr("CombatResolver: FlashcardChallengeManager is not assigned. Skipping defense challenge.");
            ExecuteEnemyAttack(false);
        }
    }
    
    // Handle flashcard answer results
    public void HandleFlashcardAnswer(bool isCorrect, SceneTree tree)
    {
        if (!_state.WaitingForFlashcard) return; // Ignore if not currently waiting for a flashcard answer
        
        _state.WaitingForFlashcard = false;
        
        // Determine which action to resolve based on the pending action state
        if (_state.PendingAction == "attack")
        {
            ExecutePlayerAttack(isCorrect); // If correct, execute attack; if incorrect, treat as missed attack
        }
        else if (_state.PendingAction == "defend")
        {
            ExecutePlayerDefense(isCorrect, tree); // If correct, treat as successful defense; if incorrect, treat as failed defense
        }
    }
    
    // Execute player attack based on success
    public void ExecutePlayerAttack(bool success)
    {
        // If attack is successful, deal damage to the first alive enemy; otherwise, log a missed attack
        if (success)
        {
            var enemyToAttack = _state.AliveEnemies.First<EnemyFSM>();
            _state.PlayerAttack.Attack(enemyToAttack);
            _uiCoordinator.LogMessage($"You attacked {enemyToAttack.Name} for {_state.PlayerAttack.BaseDamage} damage!");
        }
        else
        {
            // Play miss sound
            _state.PlayerAttack.PlayMissSound();

            _uiCoordinator.LogMessage("Attack missed!");
        }
        
        // Update health UI after attack
        _uiCoordinator.UpdateHealthUI();
        
        // Start enemy turns
        _turnController.StartEnemyTurns();
    }
    
    // Execute player defense
    private void ExecutePlayerDefense(bool success, SceneTree tree)
    {
        if (success) // Play block sound if defense successful
        {
            _state.PlayerHealth.PlayBlockSound();
        }

        // If successful defense, no damage; otherwise take full damage
        // Then advance to next enemy's turn regardless of defense outcome
        ExecuteEnemyAttack(!success);

        // Player may have died during the attack — don't advance if battle already ended
        if (!_state.InCombat) return;

        _turnController.AdvanceToNextEnemy(tree);
    }
    
    // Execute enemy attack
    private void ExecuteEnemyAttack(bool fullDamage)
    {
        var enemy = _state.AliveEnemies[_state.CurrentEnemyIndex];
        // If fullDamage is true, enemy attacks player for full damage; 
        // if false, player successfully defends and takes no damage
        if (fullDamage)
        {
            _state.EnemyAttackComponents[enemy].Attack(_state.Player);

            _uiCoordinator.LogMessage("Failed to defend! Taking full damage!");
        }
        else
        {
            // Just play attack sound
            _state.EnemyAttackComponents[enemy].PlayAttackSound();
            _uiCoordinator.LogMessage("Defended successfully! Taking no damage!");
        }
        
        // Update health UI after enemy attack
        _uiCoordinator.UpdateHealthUI();
    }
    
    // Attempt to run from battle
    public void AttemptRun(SceneTree tree)
    {
        // Randomly determine if running away is successful (50% chance)
        bool success = new Random().Next(2) == 0;
        
        // If successful, end battle with run; if failed, log failure and start enemy turns
        if (success)
        {
            _uiCoordinator.LogMessage("Successfully ran away COWARD");
            tree.CreateTimer(1.0).Timeout += () => OnBattleEndedWithRun?.Invoke(true);
        }
        else
        {
            _uiCoordinator.LogMessage("Failed to escape! Enemies gonna get you now :(");
            _turnController.StartEnemyTurns();
        }
    }
    
    // Use items (placeholder)
    public void UseItems()
    {
        // Currently not implemented, just logs a message and starts enemy turns
        _uiCoordinator.LogMessage("No items available! (Not yet implemented, wasted turn)");
        _turnController.StartEnemyTurns();
    }
}
