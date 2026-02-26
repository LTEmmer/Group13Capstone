using Godot;
using System.Collections.Generic;

public partial class EnemyExample : CharacterBody3D
{
	[Export] public Area3D DetectionArea;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		DetectionArea.BodyEntered += OnBodyEntered;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	public override void _PhysicsProcess(double delta)
	{
		MoveAndSlide();
	}

	private void OnBodyEntered(Node3D body)
	{
		if (body is Player player)
		{
			GD.Print($"Player detected by {Name}! Starting battle...");

			// For now grab this specific enemy, but eventually we will want to grab all enemies in the room
			// Maybe each room can store enemies in an enemies node so we can easily grab all enemies
			// by: GetParent().GetNode("Enemies").GetChildren() or something like that
			BattleManager.Instance.StartBattle(player, new List<EnemyExample> { this }, GetParent<Node3D>());
		}
	}
}
