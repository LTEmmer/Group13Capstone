using Godot;
using System;
using System.Threading;
using Array = Godot.Collections.Array;
[Tool]
public partial class Attack : EnemyBaseState
{
	[Export]
	public bool ranged = false;
	
	public override Godot.Collections.Array CheckRelevance(double delta){
		if (WorksLongerThan(animator.GetAnimation(StateAnimation).Length)) // state lasts till end of attack anim
		{
			//add ranged check here in the future with ranged weapons
			return [true, StateNames.moveback];
		}
		return [false];
	}

}
