using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

// Singleton battle manager that handles the entire flow of combat, 
// from initiating battles, managing turns, processing player actions, enemy AI, and ending battles.
public partial class BattleManager : Node
{
    public static BattleManager Instance { get; private set; }

	private Transform3D _originalPlayerTransform;
	private Transform3D[] _originalEnemyTransforms;
	private Node3D _battleArea;
    private BattleTransition _transition;
    private BattleUI _battleUI;
    private FlashcardChallenge _flashcardChallenge;

	// Combat state
	private Player _player;
	private AttackComponent _playerAttack;
    private HealthComponent _playerHealth;
	private List<EnemyExample> _aliveEnemies = new List<EnemyExample>();
	private Dictionary<EnemyExample, AttackComponent> _enemyAttackComponents = new Dictionary<EnemyExample, AttackComponent>();
    private Dictionary<EnemyExample, HealthComponent> _enemyHealthComponents = new Dictionary<EnemyExample, HealthComponent>();
	private Dictionary<EnemyExample, EnemyStatusComponent> _enemyStatusComponents = new Dictionary<EnemyExample, EnemyStatusComponent>();
	private bool _inCombat = false;
	private bool _waitingForAction = false;
	private bool _waitingForFlashcard = false;
	private string _pendingAction = "";
	private int _currentEnemyIndex = 0;
    private Timer _battleCooldownTimer;
    private bool _canEnterBattle = true; // Flag to control battle entry, set to false during cooldown

	public override void _Ready()
	{
        Instance = this;

        // Instantiate scenes for transition, UI, and challenge
        _transition = GD.Load<PackedScene>("res://game/ui/battle_ui/battle_transition.tscn").Instantiate<BattleTransition>();
        AddChild(_transition);
        _battleUI = GD.Load<PackedScene>("res://game/ui/battle_ui/battle_ui.tscn").Instantiate<BattleUI>();
        AddChild(_battleUI);
        _flashcardChallenge = GD.Load<PackedScene>("res://game/ui/battle_ui/flashcard_challenge.tscn").Instantiate<FlashcardChallenge>();
        AddChild(_flashcardChallenge);

        // Set up the timer for battle cooldown to prevent immediate re-entry into battle after one ends, 
        // can be adjusted as needed
        _battleCooldownTimer = new Timer();
        _battleCooldownTimer.WaitTime = 3.0; // Adjust the cooldown duration as needed
        _battleCooldownTimer.OneShot = true;
        _battleCooldownTimer.Timeout += OnBattleCooldownTimeout;
        AddChild(_battleCooldownTimer);

        if (_transition == null || _battleUI == null || _flashcardChallenge == null)
        {
            GD.PrintErr("BattleManager: Failed to load one or more UI scenes.");
        }

		// Connect UI signals if available
		if (_battleUI != null)
		{
			_battleUI.OnActionSelected += OnPlayerActionSelected;
		}

		if (_flashcardChallenge != null)
		{
			_flashcardChallenge.OnAnswerSubmitted += OnFlashcardAnswered;
		}
	}

	public void StartBattle(Player player, List<EnemyExample> enemies, Node3D room)
	{
        if (!_canEnterBattle)
        {
            GD.Print("Cannot enter battle yet. Still in cooldown.");
            return;
        }

		if (_transition == null)
		{
			GD.PrintErr("BattleManager: Transition is not assigned.");
			return;
		}

		if (player == null)
		{
			GD.PrintErr("BattleManager: Player is null.");
			return;
		}

		if (enemies == null || enemies.Count == 0)
		{
			GD.PrintErr("BattleManager: No enemies provided.");
			return;
		}

        _canEnterBattle = false; // Set to false to prevent entering another battle until cooldown ends

        // Find the battle area based off the room
        if (room != null)
        {
            var battleAreaInRoom = room.GetNodeOrNull<Node3D>("BattleArea");
            if (battleAreaInRoom != null)
            {
                _battleArea = battleAreaInRoom;
            }
            else
            {
                GD.PrintErr($"BattleManager: No BattleArea found in room {room.Name}.");
                return;
            }
        }
        else
        {
            GD.PrintErr("BattleManager: Room is null.");
            return;
        }

		_player = player;
		_aliveEnemies = enemies;

        // Disable player input immediately to prevent any actions during the transition into battle, also stop movement
        _player.SetAcceptKeyboardInput(false);
        _player.Velocity = Vector3.Zero;
        Input.MouseMode = Input.MouseModeEnum.Visible;

		// Get player attack component
		_playerAttack = player.GetNode<AttackComponent>("AttackComponent");
		if (_playerAttack == null)
		{
			GD.PrintErr("BattleManager: Player missing AttackComponent.");
			return;
		}

        // Get player health component and subscribe to death signal, log error if missing
        _playerHealth = player.GetNode<HealthComponent>("HealthComponent");
        if (_playerHealth != null)
        {
            _playerHealth._OnDeath += OnPlayerDeath;
        }
        else
        {
            GD.PrintErr("BattleManager: Player missing HealthComponent.");
        }

        // Clear old components (just in case) and get new enemy attack components, subscribe to their death signals
		_enemyAttackComponents.Clear();
        _enemyHealthComponents.Clear();
		_enemyStatusComponents.Clear();
		foreach (var enemy in _aliveEnemies)
		{   
            // Get and store attack component for each enemy, log error if missing
			var attackComp = enemy.GetNode<AttackComponent>("AttackComponent");
			if (attackComp != null)
			{
				_enemyAttackComponents.Add(enemy, attackComp);
			}
			else
			{
				GD.PrintErr($"BattleManager: Enemy {enemy.Name} missing AttackComponent.");
			}

            // Subscribe to enemy death signal to know when they die during combat, log error if missing health component
            // Note: Using a lambda here to capture the specific enemy for the death callback, 
            // this is fine since enemies will be freed after battle, disconnecting the signal
            var healthComp = enemy.GetNode<HealthComponent>("HealthComponent");
            if (healthComp != null)
            {
                healthComp._OnDeath += () => OnEnemyDeath(enemy);
                _enemyHealthComponents.Add(enemy, healthComp);
            }
            else
            {
                GD.PrintErr($"BattleManager: Enemy {enemy.Name} missing HealthComponent.");
            }

			var statusComp = enemy.GetNode<EnemyStatusComponent>("EnemyStatusComponent");
			if (statusComp != null)
			{
				_enemyStatusComponents.Add(enemy, statusComp);
			}
			else
			{
				GD.PrintErr($"BattleManager: Enemy {enemy.Name} missing EnemyStatusComponent.");
			}
		}

        // Start the transition with the first enemy as the focus
		EnemyExample focusEnemy = _aliveEnemies[0];
		_transition.Cover(focusEnemy.GlobalPosition, _player, () =>
		{
			SetupBattleArea(); // Move player and enemies to battle positions
			_transition.Reveal(() => // After transition, start combat
                {
                    InitializeCombat(); // Disable player input during transition
                }); 
		});
	}

	private void SetupBattleArea()
	{
		if (_battleArea == null)
		{
			GD.PrintErr("BattleManager: BattleAreaScene is not assigned. Combat area cannot be set up.");
			return;
		}

        // Store original positions to reset later for player and enemies
		_originalPlayerTransform = _player.GlobalTransform;
		int originalIndex = 0;
		_originalEnemyTransforms = new Transform3D[_aliveEnemies.Count];
		foreach (var enemy in _aliveEnemies)
		{
			_originalEnemyTransforms[originalIndex] = enemy.GlobalTransform;
			originalIndex++;
		}

        // Move player to player spot 
		Marker3D playerSpot = _battleArea.GetNode<Marker3D>("PlayerSpot");
		_player.GlobalTransform = playerSpot.GlobalTransform;

        // Move each enemy to their respective spots, no specific order first come first serve
		int spotIndex = 0;
		foreach (var enemy in _aliveEnemies)
		{
			enemy.GlobalTransform = _battleArea.GetNode<Marker3D>($"EnemySpot{spotIndex}").GlobalTransform;
			spotIndex++;
		}
	}

	private void InitializeCombat()
	{
        // Set combat state
		_inCombat = true;

		// Show Battle UI
		if (_battleUI != null)
		{
			_battleUI.ClearCombatLog(); // Clear any old logs
			_battleUI.AddCombatLog("Battle started!");
			_battleUI.SlideIn(); // Animate UI sliding in
		}

		// Slide in enemy status UI near their models
		foreach (var enemy in _aliveEnemies)
		{
            var status = _enemyStatusComponents[enemy];
            var health = _enemyHealthComponents[enemy];
            var attack = _enemyAttackComponents[enemy];

            status.Initialize(enemy, health, attack);
            status.SlideIn();
		}

		UpdateHealthUI(); // Initial health display

		// Start first turn
		StartPlayerTurn();
	}

	private void StartPlayerTurn()
	{
		if (!_inCombat) return; // Safety check

        // Prompt player for action and enable action buttons
		_waitingForAction = true;
		if (_battleUI != null)
		{
			_battleUI.AddCombatLog("Your turn!");
			_battleUI.SetActionsEnabled(true);
		}
	}

	private void OnPlayerActionSelected(string action)
	{
		if (!_waitingForAction) return; // Ignore if not waiting for player input

        // Check if action is valid, then process it
		_waitingForAction = false;
		_pendingAction = action;

		if (_battleUI != null)
		{
			_battleUI.AddCombatLog($"You chose to {action}!");
		}

		switch (action)
		{
			case "attack":
				StartAttackSequence();
				break;

			case "run":
				AttemptRun();
				break;

			case "items":
				UseItems();
				break;

            default:
                GD.PrintErr($"BattleManager: Unknown action '{action}' selected.");
                break;
		}
	}

	private void StartAttackSequence()
	{
		// Show flashcard challenge for attack
		if (_flashcardChallenge != null)
		{
            // Load a random card and show the challenge
			Flashcard card = _flashcardChallenge.LoadRandomCard();
			if (card != null)
			{
				_waitingForFlashcard = true;
				_flashcardChallenge.ShowChallenge(card, "Answer correctly to attack!");
			}
			else
			{
				// If no cards available, just do the attack, shouldn't happen
                GD.PrintErr("BattleManager: No flashcards available for attack challenge.");
				ExecutePlayerAttack(true);
			}
		}
		else
		{
			// If FlashcardChallenge is not assigned, just do the attack, shouldn't happen
            GD.PrintErr("BattleManager: FlashcardChallenge is not assigned. Skipping attack challenge.");
			ExecutePlayerAttack(true);
		}
	}

	private void OnFlashcardAnswered(bool isCorrect)
	{
		if (!_waitingForFlashcard) return; // Ignore if not waiting for flashcard answer

		_waitingForFlashcard = false;

        // Execute dependent on attack or defend, outcome determined by isCorrect
		if (_pendingAction == "attack")
		{
			ExecutePlayerAttack(isCorrect);
		}
		else if (_pendingAction == "defend")
		{
			ExecutePlayerDefense(isCorrect);
		}
	}

	private void ExecutePlayerAttack(bool success)
	{
		if (success)
		{
            // Attack the first enemy in the list of alive enemies, could be expanded to allow player to choose target
            var enemyToAttack = _aliveEnemies.First<EnemyExample>();
            _playerAttack.Attack(enemyToAttack);
            _battleUI?.AddCombatLog($"You attacked {enemyToAttack.Name} for {_playerAttack.BaseDamage} damage!");
		}
		else // Attack missed, no damage dealt
		{
			if (_battleUI != null)
			{
				_battleUI.AddCombatLog("Attack missed!");
			}
		}

        // Update health UI after attack for all combatants
		UpdateHealthUI();

		// Check if all enemies are dead
		if (_aliveEnemies.Count == 0)
		{
			EndBattle(true);
			return;
		}

		// Start enemy turns
		StartEnemyTurns();
	}

	private void StartEnemyTurns()
	{
		_currentEnemyIndex = 0;
		ExecuteNextEnemyTurn();
	}

	private void ExecuteNextEnemyTurn()
	{
		if (_currentEnemyIndex >= _aliveEnemies.Count)
		{
			// All enemies have taken their turn
			StartPlayerTurn();
			return;
		}

        // Get the current enemy and have them attack the player
		var enemy = _aliveEnemies[_currentEnemyIndex];

		if (_battleUI != null)
		{
			_battleUI.AddCombatLog($"{enemy.Name} attacks!");
		}

		// Give player a chance to defend with flashcard, reduce damage if successful
		_pendingAction = "defend";
		if (_flashcardChallenge != null)
		{
			Flashcard card = _flashcardChallenge.LoadRandomCard();
			if (card != null)
			{
				_waitingForFlashcard = true;
				_flashcardChallenge.ShowChallenge(card, "Answer correctly to defend!");
			}
			else
			{
				// If no cards, take full damage, shouldn't happen
                GD.PrintErr("BattleManager: No flashcards available for defense challenge.");
				ExecuteEnemyAttack(false);
			}
		}
		else
		{
            // If no FlashcardChallenge assigned, take full damage, shouldn't happen
            GD.PrintErr("BattleManager: FlashcardChallenge is not assigned. Skipping defense challenge.");
			ExecuteEnemyAttack(false);
		}
	}

	private void ExecutePlayerDefense(bool success)
	{   
        // If successful defense, reduce damage taken by DefenseReduction, otherwise take full damage
		ExecuteEnemyAttack(!success);
	}

	private void ExecuteEnemyAttack(bool fullDamage)
	{
        // If fullDamage is true, player takes full damage, otherwise no damage take
        // Can be changed to apply defense reduction later
        if (fullDamage)
        {
            // The current enemy at _currentEnemyIndex is attacking the player
            var enemy = _aliveEnemies[_currentEnemyIndex];
            _enemyAttackComponents[enemy].Attack(_player);
            _battleUI?.AddCombatLog("Failed to defend! Taking full damage!");
        }
        else
        {
            _battleUI?.AddCombatLog("Defended successfully! Taking no damage!");
        }

        // Update health UI after attack for all combatants
		UpdateHealthUI();

		// Move to next enemy and attack after a short delay to allow player to see the result of the attack
		_currentEnemyIndex++;
		GetTree().CreateTimer(0.5).Timeout += ExecuteNextEnemyTurn;
	}

	private void AttemptRun()
	{
		// 50% chance to successfully run
		bool success = new Random().Next(2) == 0;

		if (success)
		{
			if (_battleUI != null)
			{
				_battleUI.AddCombatLog("Successfully ran away COWARD");
			}

			GetTree().CreateTimer(1.0).Timeout += () => EndBattle(false, true);
		}
		else
		{
			if (_battleUI != null)
			{
				_battleUI.AddCombatLog("Failed to escape! Enemies gonna get you now :(");
			}

			// Enemy turns
			StartEnemyTurns();
		}
	}

	private void UseItems()
	{
		// Shell implementation - just a placeholder
		if (_battleUI != null)
		{
			_battleUI.AddCombatLog("No items available! (Not yet implemented, wasted turn)");
		}

		// For now, this just wastes the player's turn
		StartEnemyTurns();
	}

	private void OnEnemyDeath(EnemyExample enemy)
	{
		if (_battleUI != null)
		{
			_battleUI.AddCombatLog($"{enemy.Name} was defeated!");
		}

		UpdateHealthUI();
        _aliveEnemies.Remove(enemy);

        // Remove the corresponding components for the defeated enemy, using the name set earlier
        _enemyAttackComponents.Remove(enemy);
        _enemyHealthComponents.Remove(enemy);

        _enemyStatusComponents[enemy].SlideOut();
        _enemyStatusComponents.Remove(enemy);
	}

	private void OnPlayerDeath()
	{
		GD.Print("Player has died. Game Over.");
        EndBattle(false);
	}

	// Update player and enemy health readouts
	private void UpdateHealthUI()
	{
		// Update player health
		if (_battleUI != null)
		{
			_battleUI.UpdatePlayerHealth(_playerHealth.CurrentHealth, _playerHealth.MaxHealth);
		}

		// Update enemy status components (health)
		foreach (var enemy in _aliveEnemies)
		{
            var status = _enemyStatusComponents[enemy];
            status.SetHealth(_enemyHealthComponents[enemy].CurrentHealth, _enemyHealthComponents[enemy].MaxHealth);
		}
	}

	private void EndBattle(bool victory, bool ran = false)
	{
        // Reset combat states
		_inCombat = false;
		_waitingForAction = false;
		_waitingForFlashcard = false;
        _battleCooldownTimer.Start(); // Start cooldown timer to prevent immediate re-entry into battle

        // Show end battle message and slide out UI, then reset positions and clean up battle state
		if (_battleUI != null)
		{
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
				_battleUI.AddCombatLog("Defeat! You were overwhelmed...");
			}

            SlideOutEnemyStatus();

			// Hide UI after a delay
			GetTree().CreateTimer(2.0).Timeout += () =>
			{
				_battleUI.SlideOut(() =>
				{
                    // After UI is hidden, reset player and enemy positions
                    Input.MouseMode = Input.MouseModeEnum.Captured;

                    if (ran)
                    {
                        // If player ran, enemies are still alive, reset their positions
                        ResetPlayerEnemyPositions(_player, _aliveEnemies, true); 
                    }
                    else
                    {
                        // If player won, enemies are dead dont reset
                        ResetPlayerEnemyPositions(_player, _aliveEnemies, false); 
                        
                    }
					
					CleanupBattle();

                    // Play transition out
                    _transition.SliceOut();

                    // Re-enable player input after transition is done, use short time for better feel
                    GetTree().CreateTimer(0.5).Timeout += () =>
                    {
                        _player.SetAcceptKeyboardInput(true);
                    };
			    });
            };
		}
		else // If no BattleUI assigned, just reset positions and clean up immediately, shouldn't happen
		{
			SlideOutEnemyStatus();
			ResetPlayerEnemyPositions(_player, _aliveEnemies, true);
            _player.SetAcceptKeyboardInput(true);
			CleanupBattle();
		}
	}

    private void OnBattleCooldownTimeout()
    {
        // This function is called when the battle cooldown timer finishes, allowing the player to enter battle again
        GD.Print("Battle cooldown ended. Player can enter battle again.");
        _canEnterBattle = true;
    }

	private void CleanupBattle()
	{
		_aliveEnemies.Clear();
        _enemyAttackComponents.Clear();
		_enemyHealthComponents.Clear();
		_enemyStatusComponents.Clear();
	}

	private void SlideOutEnemyStatus()
	{
        // Slide out each enemy status
		foreach (var status in _enemyStatusComponents)
		{
			status.Value.SlideOut();
		}
	}

	private void ResetPlayerEnemyPositions(Player player, List<EnemyExample> enemies, bool enemiesAlive)
	{
		// Reset player position
		player.GlobalTransform = _originalPlayerTransform;

		// Reset enemy positions if alive
        if (enemiesAlive)
		{
			int index = 0;
			foreach (var enemy in enemies)
			{
				if (enemy is Node3D enemyNode && index < _originalEnemyTransforms.Length)
				{
					enemy.GlobalTransform = _originalEnemyTransforms[index];
					index++;
				}
			}
		}

		_originalEnemyTransforms = null;
	}

    /* For testing purposes, can be removed later, should only be used in test room to trigger battle 
    without needing to interact with an object

	public override void _Input(InputEvent @event)
	{
		if (@event.IsActionPressed("ui_accept"))
		{
			// Example input to trigger the transition for testing, should only use in test room
			Player player = GetParent().GetNode<Player>("TestRoom/Player");
			List<EnemyExample> enemies = [ GetParent().GetNode<EnemyExample>("TestRoom/EnemyExample") ];

			StartBattle(player, enemies, GetNode<Node3D>("../TestRoom")); // Pass the room node to find the battle area
		}
	}
    */
}
