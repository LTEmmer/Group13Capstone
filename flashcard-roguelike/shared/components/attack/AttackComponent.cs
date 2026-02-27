using Godot;
using System;

public partial class AttackComponent : Node
{
	[Export] public float BaseDamage = 20f;
	[Export] public float Accuracy = 100f; // percent chance to hit

	public bool Attack(Node target)
	{
		// Check if attack hits based on accuracy
		if (!IsHit())
		{
			GD.Print($"{GetParent().Name} missed!");
			return false;
		}

		// Try to get the health component from the target
		HealthComponent healthComponent = target.GetNode<HealthComponent>("HealthComponent");
		
		if (healthComponent != null)
		{
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

	private bool IsHit()
	{
		return GD.Randf() * 100f <= Accuracy;
	}
}
