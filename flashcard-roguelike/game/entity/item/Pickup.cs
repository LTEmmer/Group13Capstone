using Godot;
using System;

public partial class Pickup : Node3D
{
	[Export] public float HealAmount = 25f;
	[Export] public float DamageBonus = 10f;
	[Export] public bool PlayerOnly = true;

	private Area3D _area;

	// Returns display name and description for the inventory based on what this pickup does
	private (string Name, string Description) GetItemDisplayInfo()
	{
		bool heals = HealAmount > 0;
		bool buffsDamage = DamageBonus > 0;

		if (heals && buffsDamage)
			return ("Health Pack & Damage Buff", "Restores health and increases attack damage.");
		if (heals)
			return ("Health Pack", "Restores health.");
		if (buffsDamage)
			return ("Damage Buff", "Increases your attack damage.");
		return ("unknown", "An unknown object.");
	}

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

		// When PlayerOnly, we must find the HealthComponent that has IsPlayer (player has two in scene)
		var health = PlayerOnly ? FindPlayerHealth(root) : FindHealth(root);
		var attack = FindAttack(root);

		if (PlayerOnly)
		{
			if (health == null) return;
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

		// Log this pickup into the inventory if available
		var inventory = FindInventory(root);
		if (inventory != null)
		{
			var (name, description) = GetItemDisplayInfo();
			// inventory.AddItem(name, description);
			GD.Print($"Added '{name}' to inventory.");
		}

		GD.Print("Pickup consumed.");

		QueueFree();
	}

	private HealthComponent FindHealth(Node root)
	{
		var hc = root.GetNodeOrNull<HealthComponent>("HealthComponent");
		if (hc != null) return hc;
		return root.FindChild("HealthComponent", recursive: true, owned: false) as HealthComponent;
	}

	//Finds the HealthComponent with IsPlayer == true
	private HealthComponent FindPlayerHealth(Node root)
	{
		foreach (Node child in root.GetChildren())
		{
			if (child is HealthComponent hc && hc.IsPlayer)
				return hc;
			var found = FindPlayerHealth(child);
			if (found != null) return found;
		}
		return null;
	}

	private AttackComponent FindAttack(Node root)
	{
		var ac = root.GetNodeOrNull<AttackComponent>("AttackComponent");
		if (ac != null) return ac;

		return root.FindChild("AttackComponent", recursive: true, owned: false) as AttackComponent;
	}

	private InventoryComponent FindInventory(Node root)
	{
		var inv = root.GetNodeOrNull<InventoryComponent>("InventoryComponent");
		if (inv != null) return inv;

		return root.FindChild("InventoryComponent", recursive: true, owned: false) as InventoryComponent;
	}
}
