using Godot;
using System;
using Array = Godot.Collections.Array;
[Tool]


public partial class MoveIn : EnemyBaseState
{
	public Vector3 OriginPosition;
	[Export] 
	public float TweenDuration = 1.0F;
	public override Godot.Collections.Array CheckRelevance(double delta){
		if (WorksLongerThan(TweenDuration)) // state lasts till end of attack anim
		{
			return [true, StateNames.attack];
		}
		return [false];
	}
	public override void OnEnterState()
	{
		OriginPosition = enemy.GlobalPosition;
		CharacterBody3D Player = enemy.EnemyModel.Player;
		Vector3 playerPosition = Player.GlobalTransform.Origin;
		//enemy.LookAt(Player.GlobalPosition);
		Vector3 playerForwardDirection = -Player.GlobalTransform.Basis.Z;
		float meleeAttackDistanceFromPlayer = 2.0F;
		Vector3 meleeAttackPositionFromPlayer =
			Player.GlobalTransform.Origin + playerForwardDirection * meleeAttackDistanceFromPlayer;
		//Tween enemy to melee attack position
		float tween_duration = 0.8F;
		Tween tween = CreateTween();
		tween.TweenProperty(enemy,"global_position",meleeAttackPositionFromPlayer,tween_duration);
	}
}
