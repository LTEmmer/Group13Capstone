using Godot;

public partial class MainMenu : Control
{
	private Camera3D[] _cameras;
	private DirectionalLight3D _mainLight;
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

	// Target directions for LookAt — spread across axes for distinct light angles
	private static readonly Vector3[] LightAngles = {
		new Vector3( 0,    0,    1),  // straight forward
		new Vector3( 0,    0,   -1),  // straight back
		new Vector3( 1,    0,    0),  // hard right
		new Vector3(-1,    0,    0),  // hard left
		new Vector3( 0,   -1,    1),  // steep down-forward
		new Vector3( 1,   -1,    0),  // down-right diagonal
		new Vector3(-1,   -1,    0),  // down-left diagonal
		new Vector3( 0,   -1,    1),  // down-forward diagonal
		new Vector3( 1,    0,   -1),  // side-back diagonal
		new Vector3(-1,    0,    1),  // side-forward diagonal
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
				vp.GetNodeOrNull<Camera3D>("Cameras/Cam1"),
				vp.GetNodeOrNull<Camera3D>("Cameras/Cam2"),
				vp.GetNodeOrNull<Camera3D>("Cameras/Cam3"),
				vp.GetNodeOrNull<Camera3D>("Cameras/Cam4"),
				vp.GetNodeOrNull<Camera3D>("Cameras/Cam5"),
				vp.GetNodeOrNull<Camera3D>("Cameras/Cam6"),
				vp.GetNodeOrNull<Camera3D>("Cameras/Cam7"),
				vp.GetNodeOrNull<Camera3D>("Cameras/Cam8"),
				vp.GetNodeOrNull<Camera3D>("Cameras/Cam9"),
			};

			_mainLight = vp.GetNodeOrNull<DirectionalLight3D>("Lights/MainLight");

			foreach (var cam in _cameras)
				if (cam != null)
					cam.PhysicsInterpolationMode = PhysicsInterpolationModeEnum.Off;

			if (_mainLight != null)
				_mainLight.PhysicsInterpolationMode = PhysicsInterpolationModeEnum.Off;
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

		if (_mainLight != null)
		{
			Color lightColor = LightColors[GD.RandRange(0, LightColors.Length - 1)];
			_mainLight.LightColor = lightColor;

			var target = LightAngles[GD.RandRange(0, LightAngles.Length - 1)];
			_mainLight.LookAt(target);
			GD.Print($"Switched to camera {_activeCameraIndex}, light color {lightColor}, looking at {target}");
		}
	}
}
