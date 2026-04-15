using Godot;

public partial class MovingPlatform : AnimatableBody3D
{
	[Export] public float TravelTime = 2.5f;
	[Export] public Vector3 StartPos = Vector3.Zero;
	[Export] public Vector3 EndPos;
	private float _t = 0f;
	private bool _returning = false;

	public override void _Ready()
	{
		if (StartPos == Vector3.Zero || EndPos == Vector3.Zero)
		{
			GD.PrintErr("MovingPlatform requires StartPos and EndPos to be set.");
			return;
		}
	}

	public override void _PhysicsProcess(double delta)
	{
		_t += (float)delta / TravelTime;
		if (_t >= 1f)
		{
			_t = 0f;
			_returning = !_returning;
		}

		// SyncToPhysics (true by default on AnimatableBody3D) diffs the position
		// each frame and imparts that velocity to bodies standing on the platform.
		Position = _returning ? EndPos.Lerp(StartPos, _t) : StartPos.Lerp(EndPos, _t);
	}

	public void ApplyBoon(float percent, bool isBoon)
	{
		if (isBoon) // Make it x percent slower
		{
			TravelTime *= (1 + percent);
		}
		else // Make it x percent faster, but not less than 0.5 seconds to avoid being impossible to interact with
		{
			TravelTime = Mathf.Max(TravelTime * (1 - percent), 0.5f);
		 }
	}
}
