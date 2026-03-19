using Godot;
using System;
using Array = Godot.Collections.Array;
[Tool]
public partial class Death : EnemyBaseState
{
	private double _deathTimer = 2.5; //length of death animation
	public override Array CheckRelevance(double delta)
	{
		if(WorksLongerThan(_deathTimer)){
			enemy.QueueFree();
		}
		return [false];
	}

	public override void OnEnterState()
	{
		enemy.Velocity = Vector3.Zero;
	}
}
