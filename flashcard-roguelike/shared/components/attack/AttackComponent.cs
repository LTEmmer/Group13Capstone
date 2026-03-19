using Godot;
using System;

public partial class AttackComponent : Node
{
	[Export] public float BaseDamage = 20f;

	[Export] public AudioStream[] AttackSounds;
	[Export] public AudioStream[] MissSounds;

	private AudioStreamPlayer3D _audioPlayer;

	public override void _Ready()
	{
		// Create an AudioStreamPlayer3D for playing attack/miss sounds
		_audioPlayer = new AudioStreamPlayer3D();
		
		// Check if parent is a player node
		if (GetParent() is Player)
		{
			_audioPlayer.VolumeDb = -30f; // Reduce volume for player attacks
		}
		else
		{
			_audioPlayer.VolumeDb = -10f; // Slightly louder for enemies
		}

		GetParent().CallDeferred(Node.MethodName.AddChild, _audioPlayer);
	}

	public bool Attack(Node target)
	{
		// Try to get the health component from the target
		HealthComponent healthComponent = target.GetNode<HealthComponent>("HealthComponent");
		
		if (healthComponent != null)
		{
			// Play attack sound
			if (AttackSounds != null && AttackSounds.Length > 0)
			PlayAttackSound();

			GD.Print($"{GetParent().Name} attacked {target.Name} for {BaseDamage} damage!");
			healthComponent.TakeDamage(BaseDamage);
			return true;
		}
		else
		{
			GD.PrintErr($"Target {target.Name} does not have a HealthComponent!");
			return false;
		}
	}

	public void PlayMissSound()
	{
		if (MissSounds != null && MissSounds.Length > 0)
		{
			var missSound = MissSounds[GD.Randi() % MissSounds.Length];
			_audioPlayer.Stream = missSound;
			_audioPlayer.Play();
		}
	}

	public void PlayAttackSound()
	{
		if (AttackSounds != null && AttackSounds.Length > 0)
		{
			var attackSound = AttackSounds[GD.Randi() % AttackSounds.Length];
			_audioPlayer.Stream = attackSound;
			_audioPlayer.Play();
		}
	}
}
