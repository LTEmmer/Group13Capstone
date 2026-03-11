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
	public double TransitionTiming = 0.8;
	[Export]
	public double JumpTiming = 0.1;
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
			if (!_jumped)
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
		if (Math.Abs(angle) >= AngularSpeed * delta)
		{
			player.Velocity = player.Velocity.Rotated(Vector3.Up, Math.Sign(angle) * AngularSpeed * (float)delta);
			facingDirection = facingDirection.Rotated(Vector3.Up, Math.Sign(angle)* AngularSpeed * (float)delta);
		}
		else
		{
			player.Velocity = player.Velocity.Rotated(Vector3.Up, angle);
			facingDirection = facingDirection.Rotated(Vector3.Up, angle);
		}
		player.LookAt(player.GlobalPosition - facingDirection);
	}

	public override void OnEnterState()
	{
		player.Velocity = player.Velocity.Normalized() * Speed;
	}
}
