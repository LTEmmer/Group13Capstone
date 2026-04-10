using Godot;
using System;
using Array = Godot.Collections.Array;
[Tool]

public partial class IdleBattle : EnemyBaseState
{
	[Export] public Node3D Rig;
	public override Array CheckRelevance(double delta)
	{
		if(!enemy.EnemyModel.BattleMode){ 
			return [true,StateNames.idle]; 
		}
		return [false]; //Enemy remains in idle by default
	}

	//public override void Update(double delta)
	//{
		////enemy.LookAt(enemy.EnemyModel.Player.GlobalPosition, Vector3.Up);
	//}

	public override void OnEnterState()
	{
		if (enemy is IBossEnemy)
		{
			Rig.RotationDegrees = new Vector3(0, -180, 0);
		}

		enemy.Velocity = Vector3.Zero;
		enemy.LookAt(enemy.EnemyModel.Player.GlobalPosition);
	}
}
