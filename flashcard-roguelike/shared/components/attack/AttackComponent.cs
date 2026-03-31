using Godot;

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

		GetParent().CallDeferred(Node.MethodName.AddChild, _audioPlayer);
	}

	public bool Attack(Node target, float damageMultiplier = 1.0f)
	{
		// Try to get the health component from the target
		HealthComponent healthComponent = target.GetNode<HealthComponent>("HealthComponent");

		if (healthComponent != null)
		{
			// Play attack sound
			if (AttackSounds != null && AttackSounds.Length > 0)
			PlayAttackSound();

			// Calculate damage with multiplier and apply to target, for now only bosses will change the multiplier
			float damage = BaseDamage * damageMultiplier;
			GD.Print($"{GetParent().Name} attacked {target.Name} for {damage} damage!");
			healthComponent.TakeDamage(damage);
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
