using Godot;

public partial class DamageZone : Area3D
{
	[Export] public float Damage = 50f;
	[Export] public NodePath RespawnPointPath; // Set to the room's EnterPoint in the inspector

	[Signal] public delegate void PlayerFellEventHandler();

	public override void _Ready()
	{
		BodyEntered += OnBodyEntered;
	}

	private void OnBodyEntered(Node3D body)
	{
		if (body.Name != "Player") return;

		var health = body.GetNodeOrNull<HealthComponent>("HealthComponent");
		if (health == null) return;

		health.TakeDamage(Damage);

		// Teleport back to respawn point only if the player survived
		if (health.CurrentHealth > 0)
		{
			var respawn = GetNodeOrNull<Marker3D>(RespawnPointPath);
			if (respawn != null)
				body.GlobalPosition = respawn.GlobalPosition;
		}

		EmitSignal("PlayerFell");
	}
}
