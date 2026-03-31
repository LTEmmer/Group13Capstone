using Godot;

public partial class ScalingPlatform : StaticBody3D
{
	[Export] public float MinScale = 0.25f;
	[Export] public float MaxScale = 1.0f;
	[Export] public float ScaleTime = 2.0f; // seconds per full cycle
	[Export] public bool ScaleX = true;
	[Export] public bool ScaleY = true;
	[Export] public bool ScaleZ = true;

	private float _t = 0f;
	private float _speedMultiplier = 1.0f;

	public override void _PhysicsProcess(double delta)
	{
		_t += (float)delta * _speedMultiplier / ScaleTime;
		// Smooth cosine oscillation between MinScale and MaxScale
		float s = MinScale + (MaxScale - MinScale) * (0.5f - 0.5f * Mathf.Cos(_t * Mathf.Tau));
		Scale = new Vector3(
			ScaleX ? s : 1.0f,
			ScaleY ? s : 1.0f,
			ScaleZ ? s : 1.0f
		);
	}

	public void ApplyBoon(float percent, bool isBoon)
	{
		if (isBoon) // Make it x percent slower
		{
			ScaleTime *= (1 + percent);
		}
		else // Make it x percent faster, but not less than 0.5 seconds to avoid being impossible to interact with
		{
			ScaleTime = Mathf.Max(ScaleTime * (1 - percent), 0.5f);
		}
	}

}
