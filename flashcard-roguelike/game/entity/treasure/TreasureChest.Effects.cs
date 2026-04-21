using Godot;

public partial class TreasureChest
{
	// Rarity light — assign the room's spotlight in the Inspector
	[Export] public Light3D ChestLight;
	[Export] public Color[] RarityColors;

	// Particles
	[Export] private GpuParticles3D _particles;

	// Camera
	[Export] public Camera3D RevealCamera;
	[Export] public Marker3D[] CameraAngles;

	// Tuning
	[Export] public float LidImpulseUp       = 25f;
	[Export] public float LidImpulseHoriz    = 5f;
	[Export] public float ShakeDuration      = 0.25f;
	[Export] public float ShakeMagnitude     = 3f;
	[Export] public float IdleShakePause     = 1.0f;
	[Export] public float LightCycleDuration = 2f;
	[Export] public float InitialStepDuration = 1.75f;

	private Tween _hoverTween;
	private Tween _idleShakeTween;
	private Tween _lightCycleTween;

	private void StartLightCycle()
	{
		if (ChestLight == null)
		{
			GD.PushWarning("TreasureChest: ChestLight is not assigned, skipping light cycle.");
			return;
		}

		_lightCycleTween?.Kill();
		_lightCycleTween = CreateTween().SetLoops();
		foreach (var color in RarityColors)
		{
			_lightCycleTween.TweenProperty(ChestLight, "light_color", color, LightCycleDuration)
				.SetTrans(Tween.TransitionType.Sine).SetEase(Tween.EaseType.InOut);
		}
	}

	private void StartIdleShake()
	{
		_idleShakeTween?.Kill();
		float half = ShakeDuration * 0.25f;
		_idleShakeTween = CreateTween();
		_idleShakeTween.TweenProperty(_shakeNode, "rotation_degrees:z",  ShakeMagnitude, half);
		_idleShakeTween.TweenProperty(_shakeNode, "rotation_degrees:z", -ShakeMagnitude, half * 2f);
		_idleShakeTween.TweenProperty(_shakeNode, "rotation_degrees:z",  0f,             half);
		_idleShakeTween.TweenCallback(Callable.From(() =>
		{
			if (!IsOpen)
			{
				GetTree().CreateTimer(IdleShakePause).Timeout += () =>
				{
					if (!IsOpen)
					{
						StartIdleShake();
					}
				};
			}
		}));
	}

	private void DoAnticipationShake(System.Action onComplete)
	{
		if (ShakeDuration <= 0f)
		{
			onComplete?.Invoke();
			return;
		}

		float half = ShakeDuration * 0.25f;
		var tween = CreateTween();
		tween.TweenProperty(_shakeNode, "rotation_degrees:z",  ShakeMagnitude, half);
		tween.TweenProperty(_shakeNode, "rotation_degrees:z", -ShakeMagnitude, half * 2f);
		tween.TweenProperty(_shakeNode, "rotation_degrees:z",  0f,             half);
		tween.TweenCallback(Callable.From(() => onComplete?.Invoke()));
	}

	private void LaunchLid()
	{
		if (_lidBody == null)
		{
			return;
		}

		_lidBody.Freeze = false;

		float angle = _rng.Randf() * Mathf.Tau;
		_lidBody.ApplyCentralImpulse(new Vector3(
			Mathf.Cos(angle) * LidImpulseHoriz,
			LidImpulseUp,
			Mathf.Sin(angle) * LidImpulseHoriz
		));

		_lidBody.ApplyTorqueImpulse(new Vector3(
			_rng.RandfRange(-2f, 2f),
			_rng.RandfRange(-1f, 1f),
			_rng.RandfRange(-2f, 2f)
		));
	}

	private void StartRarityReveal(int targetIndex)
	{
		RevealCamera.Current = true;

		var tween = CreateTween();
		float stepDuration = InitialStepDuration;

		for (int i = 0; i <= targetIndex; i++)
		{
			int idx = i; // Capture for lambda

			tween.TweenCallback(Callable.From(() =>
			{
				RevealCamera.Transform = CameraAngles[idx].Transform;
				RevealCamera.ResetPhysicsInterpolation();

				if (ChestLight != null)
				{
					ChestLight.LightColor = RarityColors[idx];
				}

				if (_openSoundPlayer != null)
				{
					_openSoundPlayer.PitchScale = 1f + (idx * .05f);
					_openSoundPlayer.Play();
				}
			}));

			stepDuration /= 1.25f;
			tween.TweenInterval(stepDuration);
		}

		// Spawn items and fade light after full reveal
		tween.TweenCallback(Callable.From(() =>
		{
			SpawnItems();
			if (ChestLight != null)
			{
				CreateTween().TweenProperty(ChestLight, "light_energy", 0f, 0.5f)
					.SetTrans(Tween.TransitionType.Sine).SetEase(Tween.EaseType.In);
			}
		}));
	}
}
