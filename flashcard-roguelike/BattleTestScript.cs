using Godot;
using System;

public partial class BattleTestScript : Node
{
    /* This is a test area for components. */
	private Node player;
	private Node enemy;
	private AttackComponent playerAttack;
	private AttackComponent enemyAttack;
	private HealthComponent playerHealth;
	private HealthComponent enemyHealth;
	private AcceptDialog deathDialog;

	private const float HealAmount = 25f;

	public override void _Ready()
	{
		// Get references to player and enemy nodes
		player = GetNode("Player");
		enemy = GetNode("Enemy");

		// Get attack and health components
		playerAttack = player.GetNode<AttackComponent>("AttackComponent");
		enemyAttack = enemy.GetNode<AttackComponent>("AttackComponent");
		playerHealth = player.GetNode<HealthComponent>("HealthComponent");
		enemyHealth = enemy.GetNode<HealthComponent>("HealthComponent");

		// Create and setup death dialog
		deathDialog = new AcceptDialog();
		deathDialog.Title = "Game Over";
		deathDialog.DialogText = "You died!";
		AddChild(deathDialog);

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
