using Godot;
using System;
using Array = Godot.Collections.Array;
[Tool]
public partial class EnemyIdle : EnemyBaseState
{
	public override Array CheckRelevance(double delta)
	{
		return [false]; //Enemy remains in idle by default
	}

	public override void OnEnterState()
	{
		enemy.Velocity = Vector3.Zero;
	}
}
