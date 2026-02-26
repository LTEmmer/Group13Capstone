using Godot;
using System;

public partial class HealthComponent : Node
{
	[Signal]
	public delegate void _OnDeathEventHandler();

	[Export] public float MaxHealth = 100f;
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
		// Connect this signal from the signal pannel to function on the parent
		// node that you want it to do.
		// For example, if you want it to despawn make it run
		// .. :: queue_free
		// from the signal pannel in godot
		EmitSignal(SignalName._OnDeath);
	}
}
