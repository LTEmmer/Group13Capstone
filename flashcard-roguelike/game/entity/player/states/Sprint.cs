using Godot;
using System;
[Tool]
public partial class Sprint : BaseState
{
	private int _speed { get; set; } = 20;
	public override Godot.Collections.Array CheckRelevance(InputPackage input, double delta){
		if(player.IsOnFloor() == false){
			return [true, StateNames.midair];
		}
		return BestNextInput(input);
	}
	
	public override void Update(InputPackage input, double delta){
		VelocityCalculation(input, delta);
		player.MoveAndSlide();
	}
	
	private void VelocityCalculation(InputPackage input, double delta){
		Vector3 direction = (player.Transform.Basis * new Vector3(input.InputDirection.X, 0, input.InputDirection.Y)).Normalized();
		if (direction != Vector3.Zero)
		{
			player.Velocity = new Vector3(direction.X * _speed, player.Velocity.Y, direction.Z * _speed);
		}
	}
}
