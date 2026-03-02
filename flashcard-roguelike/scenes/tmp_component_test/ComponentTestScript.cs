using Godot;
using System;

public partial class ComponentTestScript : Node3D
{
	[Export] private Node player;
	[Export] private Node enemy;
	private AttackComponent playerAttack;
	private AttackComponent enemyAttack;
	private HealthComponent playerHealth;
	private HealthComponent enemyHealth;

	private const float HealAmount = 25f;
	public override void _Ready()
	{
		// Get attack and health components
		playerAttack = player.GetNode<AttackComponent>("AttackComponent");
		enemyAttack = enemy.GetNode<AttackComponent>("AttackComponent");
		playerHealth = player.GetNode<HealthComponent>("HealthComponent");
		enemyHealth = enemy.GetNode<HealthComponent>("HealthComponent");

		GD.Print("Battle Test Ready!");
		GD.Print("A - Player Attacks Enemy");
		GD.Print("D - Enemy Attacks Player");
		GD.Print("W - Player Heals");
		GD.Print("S - Enemy Heals");
	}

	public override void _Input(InputEvent @event)
	{
		if (@event is InputEventKey keyEvent && keyEvent.Pressed)
		{
			switch (keyEvent.Keycode)
			{
				case Key.A:
					playerAttack.Attack(enemy);
					break;
				case Key.D:
					enemyAttack.Attack(player);
					break;
				case Key.W:
					playerHealth.Heal(HealAmount);
					break;
				case Key.S:
					enemyHealth.Heal(HealAmount);
					break;
			}
		}
	}
}
