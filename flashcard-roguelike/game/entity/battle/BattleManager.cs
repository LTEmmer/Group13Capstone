using Godot;
using System.Collections.Generic;

// Main coordinator for the battle system. Delegates responsibilities to specialized managers
// while maintaining the singleton pattern and event-driven architecture.
public partial class BattleManager : Node
{
	public static BattleManager Instance { get; private set; }
	public bool IsInCombat => _state?.InCombat ?? false;
	public bool CanStartBattle => _canEnterBattle;

	// Specialized managers
	private BattleState _state;
	private BattleSetup _setup;
	private TurnController _turnController;
	private CombatResolver _combatResolver;
	private BattleUICoordinator _uiCoordinator;
	
	// UI Components
	public BattleTransition Transitions;
	public BattleUI ActiveUI;
	private FlashcardChallenge _flashcardChallenge;
	private FlashcardChallengeTrueOrFalse _flashcardChallengeTrueOrFalse;
	private FlashcardChallengeMultipleChoice _flashcardChallengeMultipleChoice;
	private FlashcardChallengeManager _flashcardChallengeManager;
	
	// Items panel
	private BattleItemsPanel _battleItemsPanel;

	// Victory menu
	private VictoryMenu _victoryMenu;
	private GameOverMenu _gameOverMenu;

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
		Transitions = GD.Load<PackedScene>("res://game/ui/battle_ui/battle_transition.tscn").Instantiate<BattleTransition>();
		AddChild(Transitions);

		ActiveUI = GD.Load<PackedScene>("res://game/ui/battle_ui/battle_ui.tscn").Instantiate<BattleUI>();
		AddChild(ActiveUI);

		_flashcardChallenge = GD.Load<PackedScene>("res://game/ui/battle_ui/flashcard_challenge.tscn").Instantiate<FlashcardChallenge>();
		AddChild(_flashcardChallenge);

		_flashcardChallengeTrueOrFalse = GD.Load<PackedScene>("res://game/ui/battle_ui/flashcard_challenge_true_false.tscn").Instantiate<FlashcardChallengeTrueOrFalse>();
		AddChild(_flashcardChallengeTrueOrFalse);

		_flashcardChallengeMultipleChoice = GD.Load<PackedScene>("res://game/ui/battle_ui/flashcard_challenge_multiple_choice.tscn").Instantiate<FlashcardChallengeMultipleChoice>();
		AddChild(_flashcardChallengeMultipleChoice);

		_victoryMenu = GD.Load<PackedScene>("res://game/ui/victory/victory_menu.tscn").Instantiate<VictoryMenu>();
		AddChild(_victoryMenu);

		_gameOverMenu = GD.Load<PackedScene>("res://game/ui/game_over/game_over_menu.tscn").Instantiate<GameOverMenu>();
		AddChild(_gameOverMenu);

		_battleItemsPanel = GD.Load<PackedScene>("res://game/ui/battle_ui/battle_items_panel.tscn").Instantiate<BattleItemsPanel>();
		AddChild(_battleItemsPanel);

		if (Transitions == null || ActiveUI == null || _flashcardChallenge == null || _flashcardChallengeTrueOrFalse == null || _flashcardChallengeMultipleChoice == null)
		{
			GD.PrintErr("BattleManager: Failed to load one or more UI scenes.");
		}

		// Initialize challenge manager
		_flashcardChallengeManager = new FlashcardChallengeManager();
		_flashcardChallengeManager.Initialize(_flashcardChallenge, _flashcardChallengeTrueOrFalse, _flashcardChallengeMultipleChoice);
		_flashcardChallengeManager.SetAnswerSubmittedCallback(OnFlashcardAnswered);

		// Initialize manager dependencies
		_uiCoordinator.Initialize(ActiveUI, _state);
		_turnController.Initialize(_state, _uiCoordinator, _combatResolver);
		_combatResolver.Initialize(_state, _uiCoordinator, _flashcardChallengeManager, _turnController);

		// Connect UI signals
		if (ActiveUI != null)
		{
			ActiveUI.OnActionSelected += OnPlayerActionSelected;
		}

		_battleItemsPanel.OnBack += OnItemsMenuBack;
		_battleItemsPanel.OnItemUsed += OnPlayerItemUsed;

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

	public void StartBattle(Player player, List<EnemyFSM> enemies, Node3D room)
	{
		if (!_canEnterBattle)
		{
			GD.Print("Cannot enter battle yet. Still in cooldown.");
			return;
		}

		if (Transitions == null)
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

		// Make visible
		Transitions.Visible = true;
		// ActiveUI.Visible = true;  // Moved to InitializeCombat

		// Play stinger and music
		AudioManager.Instance?.PlayBattleStinger();
		AudioManager.Instance?.PlayBattleMusic();

		// Initialize state with entities
		_state.Initialize(player, enemies);

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

		//Set enemies player field
		foreach (var enemy in enemies)
		{
			enemy.EnemyModel.Player = player;
		}
		// Start transition to battle, focusing on the first enemy for now 
		// (can be expanded to multiple enemies or a more dynamic focus later)
		EnemyFSM focusEnemy = enemies[0];

		// Play transition animation and initialize combat after transition completes and positions are set
		Transitions.Cover(focusEnemy.GlobalPosition, _state.Player, OnCoverTransitionComplete);
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

	private bool ValidateAndCacheEnemyComponents(List<EnemyFSM> enemies)
	{
		foreach (var enemy in enemies)
		{
			// Detect boss battles via interface
			if (enemy is IBossEnemy boss)
			{
				_state.IsBossBattle = true;
				_state.BossStreakRequired = boss.StreakRequired;
				_state.BossBlockReduction = boss.BlockReduction;
			}

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
			//Set enemyFSM model to battleMode can move elsewhere if needed
			enemy.EnemyModel.BattleMode = true;

		}

		return true;
	}

	// Called once the cover transition finishes positions entities then starts the reveal
	private void OnCoverTransitionComplete()
	{
		// Disable player input and stop movement after transition covers the player
		_state.Player.SetAcceptKeyboardInput(false);
		_state.Player.Velocity = Vector3.Zero;
		Input.MouseMode = Input.MouseModeEnum.Visible;

		_setup.SetupBattlePositions(_state.Player, _state.AliveEnemies);
		Transitions.Reveal(InitializeCombat);
	}

	private void InitializeCombat()
	{
		_state.InCombat = true;

		// Make UI visible after transition
		ActiveUI.Visible = true;

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

		if (action == "items")
		{
			OpenItemsMenu(); // Don't commit turn yet player may go back
			return;
		}

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

			default: // Should never happen since UI only allows valid options, but log an error just in case
				GD.PrintErr($"BattleManager: Unknown action '{action}' selected.");
				break;
		}
	}

	private void OpenItemsMenu()
	{
		var items = _state.Player.inventoryComponent?.UseItems;
		_battleItemsPanel.Populate(items);
		_battleItemsPanel.Visible = true;
	}

	private void OnItemsMenuBack()
	{
		_battleItemsPanel.Visible = false;
		ActiveUI.SetActionsEnabled(true);
	}

	private void OnPlayerItemUsed(ItemInstance item)
	{
		_battleItemsPanel.Visible = false;
		_state.WaitingForAction = false;
		_state.PendingAction = "items";
		_uiCoordinator.LogMessage($"Used {item.Resource.Name}!");
		_combatResolver.UseItem(item);
	}

	private void OnFlashcardAnswered(bool isCorrect)
	{
		// Delegate to combat resolver to handle the result based on the current pending action
		_combatResolver.HandleFlashcardAnswer(isCorrect, GetTree());
	}

	private void OnEnemyDeath(EnemyFSM enemy)
	{
		TaloTelemetry.TrackEnemiesDefeated();
		_uiCoordinator.LogMessage($"{enemy.Name} was defeated!");
		_uiCoordinator.UpdateHealthUI();
		_state.RemoveEnemy(enemy);

		// Check if all enemies are dead
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
		_flashcardChallengeManager.HideChallenge(); // Immediately clear any active flashcard (e.g. player died mid-challenge)
		_battleItemsPanel.Visible = false;
		_battleCooldownTimer.Start();
		
		
		// Disconnect player death signal to prevent duplication
		if (_state.PlayerHealth != null)
		{
			_state.PlayerHealth._OnDeath -= OnPlayerDeath;
		}

		// Handle UI end sequence
		_uiCoordinator.HandleBattleEndUI(victory, ran, () =>
		{
			// Hide UI after a delay
			GetTree().CreateTimer(2.0).Timeout += () =>
			{
				_uiCoordinator.SlideOutBattleUI(() =>
				{
					// Boss victory: show victory screen instead of returning to dungeon
					if (victory && _state.IsBossBattle)
					{
						_state.Reset();
						_gameOverMenu.ShowSessionStats("You defeated the boss!", true, showVictory: true);
						_victoryMenu.ShowVictory("You defeated the boss!\nCongratulations!");
						return;
					}

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
					Transitions.SliceOut();

					// Transition music
					AudioManager.Instance?.PlayDungeonMusic(1.5f);

					// Re-enable player input after transition (BEFORE state reset to avoid null reference)
					GetTree().CreateTimer(0.5).Timeout += () =>
					{
						_state.Player.SetAcceptKeyboardInput(true);
						_state.Reset();
					};
					foreach (var enemy in _state.AliveEnemies){ //a timer of some sort needs to be implemented to give the player a headstart
						enemy.EnemyModel.BattleMode = false;
					}
				});
			};
		});
		
		// When battle over if player wins activate room exit
		if(victory == true){ //victory increases difficulty
			EventManager.Instance.raise("on_battle_victory","test"); // Update difficulty 
			EventManager.Instance.raise("on_room_clear","test"); // Tell connections to open
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

	}

	private void OnBattleCooldownTimeout()
	{
		GD.Print("Battle cooldown ended. Player can enter battle again.");
		_canEnterBattle = true;
	}

	public void ForceHideUI()
	{
		if (!_state.InCombat) return;

		_state.InCombat = false;
		_state.WaitingForAction = false;
		_state.WaitingForFlashcard = false;

		_flashcardChallengeManager.HideChallenge();
		_battleItemsPanel.Visible = false;
		ActiveUI.Visible = false;
		Transitions.Visible = false;

		if (_state.PlayerHealth != null)
			_state.PlayerHealth._OnDeath -= OnPlayerDeath;

		if (_battleCooldownTimer.TimeLeft > 0)
			_battleCooldownTimer.Stop();

		_canEnterBattle = true;
		_state.Reset();
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
