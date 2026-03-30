using Godot;

public partial class MainMenu : Control
{
	private Camera3D[] _cameras;
	private SpotLight3D _light;
	private int _activeCameraIndex = 0;

	private static readonly Color[] LightColors =
	{
		new Color(1.0f, 0.2f, 0.2f),  // neon red
		new Color(1.0f, 0.5f, 0.0f),  // vivid orange
		new Color(1.0f, 0.9f, 0.1f),  // bright yellow
		new Color(0.2f, 1.0f, 0.3f),  // electric green
		new Color(0.0f, 1.0f, 0.9f),  // aqua cyan
		new Color(0.2f, 0.6f, 1.0f),  // vibrant blue
		new Color(0.6f, 0.3f, 1.0f),  // neon purple
		new Color(1.0f, 0.2f, 0.8f),  // magenta
		new Color(1.0f, 0.0f, 0.6f),  // hot pink
		new Color(0.4f, 1.0f, 0.8f),  // mint neon
		new Color(0.8f, 1.0f, 0.2f),  // lime
		new Color(0.1f, 0.4f, 1.0f),  // deep electric blue
	};

	// (position, look-at target) pairs for spotlight placement
	private static readonly (Vector3 pos, Vector3 target)[] LightAngles = {
		(new Vector3( 0.0f, 3.5f,  1.5f), new Vector3( 0,   1,    0)),  // front-top
		(new Vector3( 2.0f, 3.0f,  1.0f), new Vector3( 0,   1,    0)),  // right-top
		(new Vector3(-2.0f, 3.0f,  1.0f), new Vector3( 0,   1,    0)),  // left-top
		(new Vector3( 0.0f, 3.5f, -1.5f), new Vector3( 0,   1,    0)),  // back-top
		(new Vector3( 2.5f, 2.0f,  0.0f), new Vector3( 0,   1,    0)),  // far right
		(new Vector3(-2.5f, 2.0f,  0.0f), new Vector3( 0,   1,    0)),  // far left
		(new Vector3( 1.5f, 4.0f,  1.5f), new Vector3( 0, 0.5f,   0)),  // high front-right
		(new Vector3(-1.5f, 4.0f,  1.5f), new Vector3( 0, 0.5f,   0)),  // high front-left
	};

	private void InitializeBackgroundViewport()
	{
		var animPlayer = GetNodeOrNull<AnimationPlayer>("BackgroundViewport/SubViewport/AnimationLibrary_Godot_Standard/AnimationPlayer");
		if (animPlayer != null)
		{
			animPlayer.SpeedScale = 1.2f;
			animPlayer.Play("Dance");
			animPlayer.AnimationFinished += _ => animPlayer.Play("Dance");
		}

		var vp = GetNodeOrNull<SubViewport>("BackgroundViewport/SubViewport");
		if (vp != null)
		{
			_cameras = new Camera3D[] {
				vp.GetNodeOrNull<Camera3D>("Cam1"),
				vp.GetNodeOrNull<Camera3D>("Cam2"),
				vp.GetNodeOrNull<Camera3D>("Cam3"),
				vp.GetNodeOrNull<Camera3D>("Cam4"),
				vp.GetNodeOrNull<Camera3D>("Cam5"),
				vp.GetNodeOrNull<Camera3D>("Cam6"),
				vp.GetNodeOrNull<Camera3D>("Cam7"),
				vp.GetNodeOrNull<Camera3D>("Cam8"),
				vp.GetNodeOrNull<Camera3D>("Cam9"),
			};
			_light = vp.GetNodeOrNull<SpotLight3D>("SpotLight3D");
		}

		SwitchCamera();
		var cameraTimer = new Timer { WaitTime = 2, Autostart = true };
		cameraTimer.Timeout += SwitchCamera;
		AddChild(cameraTimer);
	}

	private void SwitchCamera()
	{
		if (_cameras == null) return;

		int next;
		do { next = GD.RandRange(0, _cameras.Length - 1); }
		while (_cameras.Length > 1 && next == _activeCameraIndex);

		if (_cameras[_activeCameraIndex] != null)
		{
			_cameras[_activeCameraIndex].Current = false;
		}

		_activeCameraIndex = next;

		if (_cameras[_activeCameraIndex] != null)
		{
			_cameras[_activeCameraIndex].Current = true;
		}

		if (_light != null)
		{
			_light.LightColor = LightColors[GD.RandRange(0, LightColors.Length - 1)];

			var (pos, target) = LightAngles[GD.RandRange(0, LightAngles.Length - 1)];
			_light.Position = pos;
			_light.LookAt(target);
		}
	}
}
