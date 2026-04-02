using Godot;

// Attach to any Node3D to make it bob up and down in place.
public partial class HoverComponent : Node3D
{
	[Export] public float Amplitude = .5f;
	[Export] public float Frequency = .1f;

	private Vector3 _basePosition;
	private float _time;

	public override void _Ready()
	{
		_basePosition = Position;
		_time = GD.RandRange(1, 1000); // Start at a random time to desync multiple hover components
	}

	public override void _Process(double delta)
	{
		_time += (float)delta;
		var pos = _basePosition;
		pos.Y += Mathf.Sin(_time * Frequency * Mathf.Tau) * Amplitude;
		Position = pos;
	}
}
