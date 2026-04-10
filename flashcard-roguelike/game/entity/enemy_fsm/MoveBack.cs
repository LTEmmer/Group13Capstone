using Godot;
using System;
using Array = Godot.Collections.Array;
[Tool]
public partial class MoveBack : EnemyBaseState
{
	[Export] 
	public MoveIn moveIn;
	[Export] 
	public float TweenDuration = 1.0F;
	
	public override Godot.Collections.Array CheckRelevance(double delta){
		if (WorksLongerThan(TweenDuration)) // state lasts till end of attack anim
		{
			return [true, StateNames.idlebattle];
		}
		return [false];
	}
	public override void OnEnterState()
	{
		Tween tween = CreateTween();
		tween.TweenProperty(enemy,"global_position",moveIn.OriginPosition,TweenDuration);
	}
	
}
