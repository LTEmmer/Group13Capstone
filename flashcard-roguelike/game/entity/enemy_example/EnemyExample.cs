using Godot;
using System.Collections.Generic;
using System.Linq;

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
			// by: GetParent().GetChildren() or something like that and put them in a list. (since they are children of enemies)
			// For example:

			/*
			List<EnemyExample> enemiesInRoom = new List<EnemyExample>();
			foreach (var child in GetParent().GetChildren())
			{
				if (child is EnemyExample enemy)
				{
					enemiesInRoom.Add(enemy);
				}
				else
				{
					GD.PrintErr($"Non-enemy child detected in enemy container: {child.Name}");
				}
			}

			You would get the room via GetParent().GetParent() assuming the structure is Room -> Enemies -> EnemyExample, 
			and then get all enemies from the Enemies node. I tested it this way and it worked
			*/
			var room = GetParent<Node3D>();
			List<EnemyExample> enemiesInRoom = GetParent<Node3D>().GetChildren().OfType<EnemyExample>().ToList();
			BattleManager.Instance.StartBattle(player, enemiesInRoom, room);
		}
	}
}
