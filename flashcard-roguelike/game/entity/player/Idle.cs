using Godot;
using System;
using System.Collections.Generic;

[Tool]
public partial class Idle : BaseState
{
	public override Godot.Collections.Array CheckRelevance(InputPackage input, double delta){
		if(player.IsOnFloor() == false){
			return [true, StateNames.midair];
		}
		return BestNextInput(input);
	}
	
	public override void OnEnterState(){
		player.Velocity = Vector3.Zero;
		player.MoveAndSlide();
	}
	
}
