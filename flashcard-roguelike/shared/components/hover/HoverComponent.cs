using Godot;

// Attach to any Node3D to make it bob up and down in place.
public partial class HoverComponent : Node3D
{
	[Export] public float Amplitude = .5f;
	[Export] public float Frequency = .1f;

	[Export] public bool RandomizeAmplitude = false;
	[Export] public float RandomizeAmplitudeMin = .2f;
	[Export] public float RandomizeAmplitudeMax = 1f;
	[Export] public bool RandomizeFrequency = false;
	[Export] public float RandomizeFrequencyMin = .05f;
	[Export] public float RandomizeFrequencyMax = .3f;

	private Vector3 _basePosition;
	private float _time;

	public override void _Ready()
	{
		if (RandomizeAmplitude)
		{
			Amplitude = (float)GD.RandRange(RandomizeAmplitudeMin, RandomizeAmplitudeMax);
		}

		if (RandomizeFrequency)
		{
			Frequency = (float)GD.RandRange(RandomizeFrequencyMin, RandomizeFrequencyMax);
		}

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
