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


	[Export] public AudioStream[] HurtSounds;
	[Export] public AudioStream[] BlockSounds;
	[Export] public AudioStream[] DeathSound;

	[Export] public Camera3D PlayerCamera;

	
	public float CurrentHealth { get; private set; }


	private AudioStreamPlayer3D _audioPlayer;

	public override void _Ready()
	{
		CurrentHealth = MaxHealth;
		// Create an AudioStreamPlayer3D for playing hurt/death sounds
		_audioPlayer = new AudioStreamPlayer3D();

		GetParent().CallDeferred(Node.MethodName.AddChild, _audioPlayer);
	}

	public void TakeDamage(float damage)
	{
		CurrentHealth -= damage;
		GD.Print($"{GetParent().Name}: Health: {CurrentHealth}/{MaxHealth}");

		if (IsPlayer)
		{
			ShakeCamera();
			FlashDamageOverlay();
		}
		else
		{
			SpawnDamageNumber(damage);
		}

		// Play hurt sound
		if (HurtSounds != null && HurtSounds.Length > 0)
		{
			var hurtSound = HurtSounds[GD.Randi() % HurtSounds.Length];
			_audioPlayer.Stream = hurtSound;
			_audioPlayer.Play();
		}

		if (CurrentHealth <= 0) Die();
	}

	private void SpawnDamageNumber(float damage)
	{
		if (GetParent() is not Node3D source)
		{
			return;
		}

		Label3D label = new Label3D();
		label.Text = $"-{Mathf.CeilToInt(damage)}";
		label.Billboard = BaseMaterial3D.BillboardModeEnum.Enabled;
		label.FontSize = 64;
		label.Modulate = new Color(1f, 0.2f, 0.2f); // Red color for damage numbers
		label.NoDepthTest = true; // Ensure it renders on top of everything

		AddChild(label);
		label.GlobalPosition = source.GlobalPosition + Vector3.Up * 1.5f;

		float endY = label.GlobalPosition.Y + 1.5f;
		var tween = label.CreateTween();
		tween.TweenProperty(label, "global_position:y", endY, 0.8f).SetTrans(Tween.TransitionType.Quad).SetEase(Tween.EaseType.Out);
		tween.Parallel().TweenProperty(label, "modulate:a", 0f, 0.8f);
		tween.TweenCallback(Callable.From(label.QueueFree));
	}

	private void ShakeCamera()
	{
		if (PlayerCamera == null)
		{
			GD.PushError("PlayerCamera not assigned in HealthComponent.");
			return;
		}

		var tween = CreateTween().SetTrans(Tween.TransitionType.Sine);
		tween.TweenProperty(PlayerCamera, "h_offset", 0.2f, 0.05f);
		tween.TweenProperty(PlayerCamera, "h_offset", -0.2f, 0.05f);
		tween.TweenProperty(PlayerCamera, "h_offset", 0f, 0.05f);
	}

	private void FlashDamageOverlay()
	{
		ColorRect overlay = new ColorRect()
		{
			Color = new Color(1f, 0f, 0f, 0.35f),
			MouseFilter = Control.MouseFilterEnum.Ignore
		};
		overlay.SetAnchorsPreset(Control.LayoutPreset.FullRect);

		CanvasLayer canvasLayer = new CanvasLayer() { Layer = 10 };
		canvasLayer.AddChild(overlay);
		AddChild(canvasLayer);

		var tween = overlay.CreateTween();
		tween.TweenProperty(overlay, "color:a", 0f, 0.4f);
		tween.TweenCallback(Callable.From(canvasLayer.QueueFree));
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
			// Emit enemy death signal for non-player deaths
			EmitSignal(SignalName.EnemyDied);
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
