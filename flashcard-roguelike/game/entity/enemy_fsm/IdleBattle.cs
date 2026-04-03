using Godot;
using System;
using Array = Godot.Collections.Array;
[Tool]

public partial class IdleBattle : EnemyBaseState
{
	public override Array CheckRelevance(double delta)
	{
		if(!enemy.EnemyModel.BattleMode){ 
			return [true,StateNames.idle]; 
		}
		return [false]; //Enemy remains in idle by default
	}

	public override void Update(double delta)
	{
		Vector3 playerDirection = new Vector3(-enemy.EnemyModel.Player.GlobalPosition.X,
			enemy.EnemyModel.Player.GlobalPosition.Y, -enemy.EnemyModel.Player.GlobalPosition.Z);
		enemy.LookAt(playerDirection,Vector3.Up);
	}

	public override void OnEnterState()
	{
		enemy.Velocity = Vector3.Zero;
	}
}
