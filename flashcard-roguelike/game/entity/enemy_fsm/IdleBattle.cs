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
		enemy.LookAt(enemy.EnemyModel.Player.GlobalPosition,Vector3.Up);
	}

	public override void OnEnterState()
	{
		enemy.Velocity = Vector3.Zero;
	}
}
