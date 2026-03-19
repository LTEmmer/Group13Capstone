using Godot;
using System;

public partial class HealthComponent : Node
{
	[Signal]
	public delegate void _OnDeathEventHandler();
	
	[Signal]
	public delegate void PlayerDiedEventHandler();

	[Export] public float MaxHealth = 100f;
	[Export] public bool IsPlayer = false;
	[Export] public AudioStream[] HurtSounds;
	[Export] public AudioStream[] BlockSounds;
	[Export] public AudioStream[] DeathSound;
	
	public float CurrentHealth { get; private set; }

	private AudioStreamPlayer3D _audioPlayer;

	public override void _Ready()
	{
		CurrentHealth = MaxHealth;
		// Create an AudioStreamPlayer3D for playing hurt/death sounds
		_audioPlayer = new AudioStreamPlayer3D();

		if (IsPlayer)
		{
			_audioPlayer.VolumeDb = -30f; // Make player death sound louder
		}
		else
		{
			_audioPlayer.VolumeDb = -10f; // Reduce volume for enemy sounds
		}

		GetParent().CallDeferred(Node.MethodName.AddChild, _audioPlayer);
	}

	public void TakeDamage(float damage)
	{
		CurrentHealth -= damage;
		GD.Print($"{GetParent().Name}: Health: {CurrentHealth}/{MaxHealth}");

		// Play hurt sound 
		if (HurtSounds != null && HurtSounds.Length > 0)
		{
			var hurtSound = HurtSounds[GD.Randi() % HurtSounds.Length];
			_audioPlayer.Stream = hurtSound;
			_audioPlayer.Play();
		}

		if (CurrentHealth <= 0) Die();
	}

	public void Heal(float amount)
	{
		CurrentHealth = Mathf.Min(CurrentHealth + amount, MaxHealth);
		GD.Print($"{GetParent().Name}: Health: {CurrentHealth}/{MaxHealth}");
	}

	private async void Die()
	{
		GD.Print($"{GetParent().Name} died!");

		// Play death sound
		if (DeathSound != null && DeathSound.Length > 0)
		{
			var deathSound = DeathSound[GD.Randi() % DeathSound.Length];
			_audioPlayer.Stream = deathSound;

			_audioPlayer.Play();

			if (IsPlayer)
			{
				// Wait for death sound to finish before showing game over
				await ToSignal(_audioPlayer, "finished");
			}
			else
			{
				// Reparent so sound plays even after entity is removed
				_audioPlayer.Finished += _audioPlayer.QueueFree;
				_audioPlayer.Reparent(GetTree().Root);
			}
		}
		
		if (IsPlayer)
		{
			// Trigger game over for player death
			GD.Print("Player has died - Game Over!");
			EmitSignal(SignalName.PlayerDied);
			ShowGameOver();
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

	public void PlayBlockSound()
	{
		if (IsPlayer) // Only play block sound for player
		{
			if (BlockSounds != null && BlockSounds.Length > 0)
			{
				var blockSound = BlockSounds[GD.Randi() % BlockSounds.Length];
				_audioPlayer.Stream = blockSound;
				_audioPlayer.Play();
			}
		}
	}
}
