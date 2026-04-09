using Godot;
using System;
using Array = Godot.Collections.Array;

[Tool]
public partial class Midair : BaseState
{
	[Export]
	public float DeltaVectorLength = 0.1F;
	[Export]
	public float LandingHeight = 1.15F;
	[Export]
	public RayCast3D DownCastRay;
	[Export]
	public ModifierBoneTarget3D HipsAttachment;
	public Vector3 JumpDirection;
	
	private const float GRAVITY_GOING_DOWN = 30.0F;
	
	public override Array CheckRelevance(InputPackage input, double delta)
	{
		Vector3 floorPoint = DownCastRay.GetCollisionPoint();
		if (input.actions.Contains(StateNames.jump) && InputCanBePaid(StateNames.jump))  
		{
			return [true, StateNames.jump];
		}
		else if (HipsAttachment.GlobalPosition.DistanceTo(floorPoint) < LandingHeight)
		{
			Vector3 xzVelocity = player.Velocity;
			xzVelocity.Y = 0;
			if (xzVelocity.LengthSquared() >= 10)
			{
				return [true, StateNames.landing]; // change to landing sprint in the future if needed
			}
			return [true, StateNames.landing];
		}
		else
		{
			return [false];
		}
	}

	public override void Update(InputPackage input, double delta)
	{
		RotationalVelocityCalculation(input, delta);
		Vector3 velocity = player.Velocity;
		velocity.Y -= GRAVITY_GOING_DOWN * (float)delta;
		player.Velocity = velocity;
		player.MoveAndSlide();
	}

	public void RotationalVelocityCalculation(InputPackage input, double delta) //Gives more aircontrol but going to refine later
	{
		Vector3 direction = (player.Transform.Basis * new Vector3(input.InputDirection.X, 0, input.InputDirection.Y)).Normalized();
		Vector3 InputDeltaVector = direction * DeltaVectorLength;
		JumpDirection = (JumpDirection + InputDeltaVector).LimitLength(player.Velocity.Length());
		Vector3 NewVelocity = (player.Velocity + InputDeltaVector).LimitLength(player.Velocity.Length());
		player.Velocity = NewVelocity;
	}

	public override void OnEnterState()
	{
		JumpDirection = player.Basis.Z * Math.Clamp(player.Velocity.Length(), 1, 999999);
		JumpDirection.Y = 0;
	}
}
