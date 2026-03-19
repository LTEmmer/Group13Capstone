using Godot;
using System;

public partial class HealthComponent : Node
{
	[Signal]
	public delegate void _OnDeathEventHandler();
	
	[Signal]
	public delegate void PlayerDiedEventHandler();
	
	[Signal]
	public delegate void EnemyDiedEventHandler();
	
	[Export] public float MaxHealth = 100f;
	[Export] public bool IsPlayer = false;
	[Export] public bool IsEnemy = false;
	
	public float CurrentHealth { get; private set; }

	public override void _Ready()
	{
		CurrentHealth = MaxHealth;
	}

	public void TakeDamage(float damage)
	{
		CurrentHealth -= damage;
		GD.Print($"{GetParent().Name}: Health: {CurrentHealth}/{MaxHealth}");

		if (CurrentHealth <= 0) Die();
	}

	public void Heal(float amount)
	{
		CurrentHealth = Mathf.Min(CurrentHealth + amount, MaxHealth);
		GD.Print($"{GetParent().Name}: Health: {CurrentHealth}/{MaxHealth}");
	}

	private void Die()
	{
		GD.Print($"{GetParent().Name} died!");
		
		if (IsPlayer)
		{
			// Trigger game over for player death
			GD.Print("Player has died - Game Over!");
			EmitSignal(SignalName.PlayerDied);
			ShowGameOver();
		}
		else if (IsEnemy)
		{
			EmitSignal(SignalName.EnemyDied);
		}
		else
		{
			// Non-player entities get removed
			GetParent().QueueFree();
		}
		
		EmitSignal(SignalName._OnDeath);
	}
	
	private void ShowGameOver()
	{
		// Try to find GameOverMenu in the scene tree
		var gameOverMenu = GetTree().Root.GetNodeOrNull<CanvasLayer>("GameOverMenu");
		
		if (gameOverMenu == null)
		{
			// Load and instantiate the game over menu
			var gameOverScene = GD.Load<PackedScene>("res://game/ui/game_over/game_over_menu.tscn");
			if (gameOverScene != null)
			{
				gameOverMenu = gameOverScene.Instantiate<CanvasLayer>();
				GetTree().Root.AddChild(gameOverMenu);
			}
		}
		
		// Show the game over screen
		if (gameOverMenu != null && gameOverMenu.HasMethod("ShowGameOver"))
		{
			gameOverMenu.Call("ShowGameOver", "You have fallen in the dungeon...");
		}
	}
}
