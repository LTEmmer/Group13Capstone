using Godot;

public partial class BlinkingPlatform : StaticBody3D
{
	[Export] public float OnDuration = 1.0f;  // seconds visible
	[Export] public float OffDuration = 1.0f; // seconds hidden
	[Export] public bool StartsOn = true;

	private float _timer = 0f;
	private bool _isOn;
	private CollisionShape3D _collision;
	private MeshInstance3D _mesh;

	public override void _Ready()
	{
		_isOn = StartsOn;
		_collision = GetNodeOrNull<CollisionShape3D>("CollisionShape3D");
		_mesh = GetNodeOrNull<MeshInstance3D>("MeshInstance3D");
		SetState(_isOn);
	}

	public override void _PhysicsProcess(double delta)
	{
		_timer += (float)delta;
		float duration = _isOn ? OnDuration : OffDuration;
		if (_timer >= duration)
		{
			_timer = 0f;
			_isOn = !_isOn;
			SetState(_isOn);
		}
	}

	public void ApplyBoon(float percent, bool isBoon)
	{
		if (isBoon) // Make it x percent longer
		{
			OnDuration *= (1 + percent);
		}
		else // Make it x percent shorter, but not less than 0.2 seconds to avoid being impossible to interact with
		{
			OnDuration = Mathf.Max(OnDuration * (1 - percent), 0.2f);
		}
	}

	private void SetState(bool on)
	{
		if (_collision != null) _collision.Disabled = !on;
		if (_mesh != null) _mesh.Visible = on;
	}
}
