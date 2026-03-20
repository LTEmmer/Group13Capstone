using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class EnemyFSM : CharacterBody3D
{
	[Export]
	public AttackComponent attackComponent;
	[Export]
	public HealthComponent healthComponent;
	[Export] 
	public EModel EnemyModel;
	[Export] public Area3D DetectionArea;
	
	public override void _Ready(){
		healthComponent.EnemyDied += OnEnemyDeath;
		DetectionArea.BodyEntered += OnBodyEntered;
	}

	public override void _PhysicsProcess(double delta)
	{
		EnemyModel.Update(delta);
	}

	private void OnBodyEntered(Node3D body){
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
			var room = GetParent<Node3D>().GetParent<Node3D>();
			List<EnemyFSM> enemiesInRoom = GetParent<Node3D>().GetChildren().OfType<EnemyFSM>().ToList();
			BattleManager.Instance.StartBattle(player, enemiesInRoom, room);
		}
	}
	private void OnEnemyDeath(){
		EnemyModel.SwitchTo(StateNames.death);
	}
}
