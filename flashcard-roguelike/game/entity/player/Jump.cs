using Godot;
using System;
using Array = Godot.Collections.Array;

[Tool]
public partial class Jump : BaseState
{
	[Export]
	public float Speed = 3.0F;
	[Export] 
	public float VerticalSpeedAdded = 10.0F;
	[Export]
	public float AngularSpeed = 7.0F;
	[Export]
	public double TransitionTiming = 0.5;
	[Export]
	public double JumpTiming = 0.2;
	
	private bool _jumped = false;

	public override Array CheckRelevance(InputPackage input, double delta)
	{
		if (WorksLongerThan(TransitionTiming))
		{
			_jumped = false;
			return [true, StateNames.midair];
		}
		else
		{
			return [false];
		}
	}

	public override void Update(InputPackage input, double delta)
	{
		RotationalVelocityCalculation(input, delta);
		if (WorksLongerThan(JumpTiming))
		{
			if (_jumped == false)
			{
				Vector3 velocity = player.Velocity;
				velocity.Y += VerticalSpeedAdded;
				player.Velocity = velocity;
				_jumped = true;
			}
		}

		player.MoveAndSlide();
	}

	public void RotationalVelocityCalculation(InputPackage input, double delta)
	{
		Vector3 direction = (player.Transform.Basis * new Vector3(input.InputDirection.X, 0, input.InputDirection.Y)).Normalized();
		Vector3 facingDirection = player.Basis.Z;
		float angle = facingDirection.SignedAngleTo(direction,Vector3.Up);
		if (Math.Abs(angle) >= AngularSpeed * (float)delta)
		{
			player.Velocity = player.Velocity.Rotated(Vector3.Up, Math.Sign(angle) * AngularSpeed * (float)delta);
		}
		else
		{
			player.Velocity = player.Velocity.Rotated(Vector3.Up, angle);
		}
		
	}

	public override void OnEnterState()
	{
		player.staminaComponent.CurrentStamina -= StaminaDrain; //Update player stamina
		player.Velocity = player.Velocity.Normalized() * Speed;
	}
}
