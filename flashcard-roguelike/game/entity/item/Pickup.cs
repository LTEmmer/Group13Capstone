using Godot;
using System;

public partial class Pickup : Node3D
{
	[Export] public float HealAmount = 25f;
	[Export] public float DamageBonus = 10f;
	[Export] public bool PlayerOnly = true;

	private Area3D _area;

	public override void _Ready()
	{
		_area = GetNodeOrNull<Area3D>("Area3D");
		if (_area == null)
		{
			GD.PrintErr($"{Name}: Missing child Area3D");
			return;
		}
		_area.BodyEntered += OnBodyEntered;
	}

private void OnBodyEntered(Node3D body)
{
	GD.Print("Pickup triggered by: " + body.Name);

	Node3D root = body;
	while (root != null && FindHealth(root) == null && root.GetParent() is Node3D parent)
		root = parent;

	if (root == null) root = body;

	var health = FindHealth(root);
	var attack = FindAttack(root);

	if (PlayerOnly)
	{
		if (health == null || !health.IsPlayer) return;
	}

	if (health != null)
	{
		GD.Print($"[BEFORE HEAL] {root.Name} HP: {health.CurrentHealth}/{health.MaxHealth}");
		health.Heal(HealAmount);
		GD.Print($"[AFTER HEAL] {root.Name} HP: {health.CurrentHealth}/{health.MaxHealth}");
	}

	if (attack != null)
	{
		GD.Print($"[BEFORE BONUS] {root.Name} Damage: {attack.BaseDamage}");
		attack.BaseDamage += DamageBonus;
		GD.Print($"[AFTER BONUS] {root.Name} Damage: {attack.BaseDamage}");
	}

	GD.Print("Pickup consumed.");

	QueueFree();
}

	private HealthComponent FindHealth(Node root)
	{
		// Try common direct child name first
		var hc = root.GetNodeOrNull<HealthComponent>("HealthComponent");
		if (hc != null) return hc;

		// Fallback: search any child named HealthComponent
		return root.FindChild("HealthComponent", recursive: true, owned: false) as HealthComponent;
	}

	private AttackComponent FindAttack(Node root)
	{
		var ac = root.GetNodeOrNull<AttackComponent>("AttackComponent");
		if (ac != null) return ac;

		return root.FindChild("AttackComponent", recursive: true, owned: false) as AttackComponent;
	}
}
