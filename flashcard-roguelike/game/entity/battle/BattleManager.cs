using Godot;
using System;
using System.Collections.Generic;

// Main coordinator for the battle system. Delegates responsibilities to specialized managers
// while maintaining the singleton pattern and event-driven architecture.
public partial class BattleManager : Node
{
	public static BattleManager Instance { get; private set; }

	// Specialized managers
	private BattleState _state;
	private BattleSetup _setup;
	private TurnController _turnController;
	private CombatResolver _combatResolver;
	private BattleUICoordinator _uiCoordinator;
	
	// UI Components
	private BattleTransition _transition;
	private BattleUI _battleUI;
	private FlashcardChallenge _flashcardChallenge;
	private FlashcardChallengeTrueOrFalse _flashcardChallengeTrueOrFalse;
	private FlashcardChallengeMultipleChoice _flashcardChallengeMultipleChoice;
	private FlashcardChallengeManager _flashcardChallengeManager;
	
	// Cooldown management
	private Timer _battleCooldownTimer;
	private bool _canEnterBattle = true;

	public override void _Ready()
	{
		Instance = this;

		// Initialize specialized managers
		_state = new BattleState();
		_setup = new BattleSetup();
		_turnController = new TurnController();
		_combatResolver = new CombatResolver();
		_uiCoordinator = new BattleUICoordinator();

		// Load UI scenes
		_transition = GD.Load<PackedScene>("res://game/ui/battle_ui/battle_transition.tscn").Instantiate<BattleTransition>();
		AddChild(_transition);

		_battleUI = GD.Load<PackedScene>("res://game/ui/battle_ui/battle_ui.tscn").Instantiate<BattleUI>();
		AddChild(_battleUI);

		_flashcardChallenge = GD.Load<PackedScene>("res://game/ui/battle_ui/flashcard_challenge.tscn").Instantiate<FlashcardChallenge>();
		AddChild(_flashcardChallenge);

		_flashcardChallengeTrueOrFalse = GD.Load<PackedScene>("res://game/ui/battle_ui/flashcard_challenge_true_false.tscn").Instantiate<FlashcardChallengeTrueOrFalse>();
		AddChild(_flashcardChallengeTrueOrFalse);

		_flashcardChallengeMultipleChoice = GD.Load<PackedScene>("res://game/ui/battle_ui/flashcard_challenge_multiple_choice.tscn").Instantiate<FlashcardChallengeMultipleChoice>();
		AddChild(_flashcardChallengeMultipleChoice);

		if (_transition == null || _battleUI == null || _flashcardChallenge == null || _flashcardChallengeTrueOrFalse == null || _flashcardChallengeMultipleChoice == null)
		{
			GD.PrintErr("BattleManager: Failed to load one or more UI scenes.");
		}

		// Initialize challenge manager
		_flashcardChallengeManager = new FlashcardChallengeManager();
		_flashcardChallengeManager.Initialize(_flashcardChallenge, _flashcardChallengeTrueOrFalse, _flashcardChallengeMultipleChoice);
		_flashcardChallengeManager.SetAnswerSubmittedCallback(OnFlashcardAnswered);

		// Initialize manager dependencies
		_uiCoordinator.Initialize(_battleUI, _state);
		_turnController.Initialize(_state, _uiCoordinator, _combatResolver);
		_combatResolver.Initialize(_state, _uiCoordinator, _flashcardChallengeManager, _turnController);

		// Connect UI signals
		if (_battleUI != null)
		{
			_battleUI.OnActionSelected += OnPlayerActionSelected;
		}

		if (_flashcardChallenge != null)
		{
			_flashcardChallenge.OnAnswerSubmitted += OnFlashcardAnswered;
		}

		// Connect combat resolver events
		_combatResolver.OnBattleEnded += (victory) => EndBattle(victory, false);
		_combatResolver.OnBattleEndedWithRun += (success) => EndBattle(false, success);

		// Set up battle cooldown timer
		_battleCooldownTimer = new Timer();
		_battleCooldownTimer.WaitTime = 3.0;
		_battleCooldownTimer.OneShot = true;
		_battleCooldownTimer.Timeout += OnBattleCooldownTimeout;
		AddChild(_battleCooldownTimer);
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

		if (enemies == null || enemies.Count <= 0 || enemies.Count > 3)
		{
			GD.PrintErr("BattleManager: No enemies provided or too many provided.");
			return;
		}

		_canEnterBattle = false;

		// Find battle area
		if (!_setup.FindBattleArea(room))
		{
			GD.PrintErr("BattleManager: Failed to find battle area in the room.");
			return;
		}

		// Initialize state with entities
		_state.Initialize(player, enemies);

		// Disable player input and stop movement
		player.SetAcceptKeyboardInput(false);
		player.Velocity = Vector3.Zero;
		Input.MouseMode = Input.MouseModeEnum.Visible;

		// Get and validate player components
		if (!ValidateAndCachePlayerComponents(player))
		{
			return;
		}

		// Get and validate enemy components
		if (!ValidateAndCacheEnemyComponents(enemies))
		{
			return;
		}

		// Start transition to battle, focusing on the first enemy for now 
		// (can be expanded to multiple enemies or a more dynamic focus later)
		EnemyExample focusEnemy = enemies[0];

		// Play transition animation and initialize combat after transition completes and positions are set
		_transition.Cover(focusEnemy.GlobalPosition, player, () =>
		{
			_setup.SetupBattlePositions(player, _state.AliveEnemies);
			_transition.Reveal(() =>
			{
				InitializeCombat();
			});
		});
	}

	private bool ValidateAndCachePlayerComponents(Player player)
	{
		// Get player attack component
		_state.PlayerAttack = player.GetNode<AttackComponent>("AttackComponent");
		if (_state.PlayerAttack == null)
		{
			GD.PrintErr("BattleManager: Player missing AttackComponent.");
			return false;
		}

		// Get player health component and subscribe to death signal
		_state.PlayerHealth = player.GetNode<HealthComponent>("HealthComponent");
		if (_state.PlayerHealth != null)
		{
			_state.PlayerHealth._OnDeath += OnPlayerDeath;
		}
		else
		{
			GD.PrintErr("BattleManager: Player missing HealthComponent.");
			return false;
		}

		return true;
	}

	private bool ValidateAndCacheEnemyComponents(List<EnemyExample> enemies)
	{
		foreach (var enemy in enemies)
		{
			// Get attack component
			var attackComp = enemy.GetNode<AttackComponent>("AttackComponent");
			if (attackComp != null)
			{
				_state.EnemyAttackComponents.Add(enemy, attackComp);
			}
			else
			{
				GD.PrintErr($"BattleManager: Enemy {enemy.Name} missing AttackComponent.");
			}

			// Get health component and subscribe to death signal
			var healthComp = enemy.GetNode<HealthComponent>("HealthComponent");
			if (healthComp != null)
			{
				healthComp._OnDeath += () => OnEnemyDeath(enemy);
				_state.EnemyHealthComponents.Add(enemy, healthComp);
			}
			else
			{
				GD.PrintErr($"BattleManager: Enemy {enemy.Name} missing HealthComponent.");
			}

			// Get status component
			var statusComp = enemy.GetNode<EnemyStatusComponent>("EnemyStatusComponent");
			if (statusComp != null)
			{
				_state.EnemyStatusComponents.Add(enemy, statusComp);
			}
			else
			{
				GD.PrintErr($"BattleManager: Enemy {enemy.Name} missing EnemyStatusComponent.");
			}
		}

		return true;
	}

	private void InitializeCombat()
	{
		_state.InCombat = true;

		// Initialize UI
		_uiCoordinator.InitializeBattleUI();

		// Initialize enemy status UI
		_setup.InitializeEnemyStatusUI(_state.AliveEnemies, _state.EnemyHealthComponents, 
			_state.EnemyAttackComponents, _state.EnemyStatusComponents);

		// Update health UI to show initial values
		_uiCoordinator.UpdateHealthUI();

		// Start first turn
		_turnController.StartPlayerTurn();
	}

	private void OnPlayerActionSelected(string action)
	{
		if (!_state.WaitingForAction) return; // safety

		// Set state to wait for flashcard answer if action requires it, otherwise execute immediately
		_state.WaitingForAction = false;
		_state.PendingAction = action;

		_uiCoordinator.LogMessage($"You chose to {action}!");

		switch (action)
		{
			case "attack":
				_combatResolver.StartAttackSequence();
				break;

			case "run":
				_combatResolver.AttemptRun(GetTree());
				break;

			case "items":
				_combatResolver.UseItems(); // Does nothing for now
				break;

			default: // Should never happen since UI only allows valid options, but log an error just in case
				GD.PrintErr($"BattleManager: Unknown action '{action}' selected.");
				break;
		}
	}

	private void OnFlashcardAnswered(bool isCorrect)
	{
		// Delegate to combat resolver to handle the result based on the current pending action
		_combatResolver.HandleFlashcardAnswer(isCorrect, GetTree());
	}

	private void OnEnemyDeath(EnemyExample enemy)
	{
		_uiCoordinator.LogMessage($"{enemy.Name} was defeated!");
		_uiCoordinator.UpdateHealthUI();
		_state.RemoveEnemy(enemy);

		// Check if all enemies are dead
		// Redundant check since combat resolver also checks after each attack, just in case
		if (_state.AliveEnemies.Count == 0)
		{
			EndBattle(true, false);
		}
	}

	private void OnPlayerDeath()
	{
		GD.Print("Player has died. Game Over.");
		EndBattle(false);
	}

	private void EndBattle(bool victory, bool ran = false)
	{
		_state.InCombat = false;
		_state.WaitingForAction = false;
		_state.WaitingForFlashcard = false;
		_battleCooldownTimer.Start();

		// Handle UI end sequence
		_uiCoordinator.HandleBattleEndUI(victory, ran, () =>
		{
			// Hide UI after a delay
			GetTree().CreateTimer(2.0).Timeout += () =>
			{
				_uiCoordinator.SlideOutBattleUI(() =>
				{
					// Reset mouse mode only if we're still alive
					if (!(victory == false && ran == false))
					{
						Input.MouseMode = Input.MouseModeEnum.Captured;
					}

					// Reset positions
					if (ran)
					{
						_setup.ResetPositions(_state.Player, _state.AliveEnemies, true);
					}
					else
					{
						_setup.ResetPositions(_state.Player, _state.AliveEnemies, false);
					}

					// Play transition out
					_transition.SliceOut();

					// Re-enable player input after transition (BEFORE state reset to avoid null reference)
					GetTree().CreateTimer(0.5).Timeout += () =>
					{
						_state.Player.SetAcceptKeyboardInput(true);
						_state.Reset();
					};
				});
			};
		});
		
		// When battle over if player wins activate room exit
		if(victory == true){ //victory increases difficulty
			EventManager.Instance.raise("on_battle_victory","test"); //No arg needed just added placeholder for now
			return;
		}
		if(ran == true){ //running lowers difficulty
			EventManager.Instance.raise("on_ran_from_battle","test");
			return;
		}
		if(victory == false)//losing a battle resets difficulty
		{
			EventManager.Instance.raise("on_battle_lost","test");
			return;
		}
		
		// ADEMAR: Here you can put any other code that needs to be run immediately when battle ends, 
		// for whatever your event manager needs to do.
		// All that is done here is resetting state, playing UI, and resetting positions.
		// You may also want to look at the OnDeath handlers for player and enemy to see how they trigger battle 
		// end when one side dies, and make sure to add any necessary logic there as well, (like QueueFreeing enemies, etc.)
		// since right now nothing actually queuefrees the enemy after death, it just slides out the UI and disables their status display. 
		// For now all combat is independent of room, enemies act as combat initiators (the ExampleEnemy) 
		// and connections spawn regardless of victory or defeat, though you could modify some stuff to disable 
		// connections until victory where your event manager would trigger them to open, 
		// or something like that through an event here. 
	}

	private void OnBattleCooldownTimeout()
	{
		GD.Print("Battle cooldown ended. Player can enter battle again.");
		_canEnterBattle = true;
	}

	/* For testing purposes, can be removed later
	public override void _Input(InputEvent @event)
	{
		if (@event.IsActionPressed("ui_accept"))
		{
			Player player = GetParent().GetNode<Player>("TestRoom/Player");
			List<EnemyExample> enemies = [ GetParent().GetNode<EnemyExample>("TestRoom/EnemyExample") ];
			StartBattle(player, enemies, GetNode<Node3D>("../TestRoom"));
		}
	}
	*/
}
