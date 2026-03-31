using Godot;
using System;
using Array = Godot.Collections.Array;
[Tool]
public partial class Landing : BaseState
{
	private const float TRANSITION_TIME = 0.2F;
	private const float GRAVITY_GOING_DOWN = 29.4F;

	public override void OnEnterState()
	{
		player.PlayLandSound();
	}

	public override Array CheckRelevance(InputPackage input, double delta)
	{
		if (WorksLongerThan(TRANSITION_TIME))
		{
			return BestNextInput(input);
		}
		else
		{
			return [false];
		}

	}

	public override void Update(InputPackage input, double delta)
	{
		Vector3 UpdatedVelocity = player.Velocity;
		UpdatedVelocity.Y -= GRAVITY_GOING_DOWN * (float)delta;
		UpdatedVelocity.Z = 0.0F;
		UpdatedVelocity.X = 0.0F;
		player.Velocity = UpdatedVelocity;
		player.MoveAndSlide();
	}
}
