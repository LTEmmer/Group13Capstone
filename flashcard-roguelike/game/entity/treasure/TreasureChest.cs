using Godot;
using System.Collections.Generic;
using System.Data;
using System.Linq;

public partial class TreasureChest : Interactable
{
	[Export] public Godot.Collections.Array<ItemResource> LootPool = new();
	[Export] public PackedScene ItemScene;
	[Export] public int MinItems        = 1;
	[Export] public int MaxItems        = 3;
	[Export] public float SpawnRadius   = 1.5f;
	[Export] public float SpawnHeight   = 0.5f;
	[Export] public float SpawnArcStart = 90f;
	[Export] public float SpawnArcEnd   = 260f;
	[Export] public bool IsOpen { get; private set; } = false;

	[Signal] public delegate void ChestOpenedEventHandler(TreasureChest chest);

	[Export] private Label3D _label;
	[Export] private RigidBody3D _lidBody;

	// Sound
	[Export] public AudioStream OpenSound;
	[Export] private AudioStreamPlayer3D _openSoundPlayer;

	// Rarity light assign the room's spotlight in the Inspector
	[Export] public Light3D ChestLight;
	[Export] public Color[] RarityColors;

	// Particles
	[Export] private GpuParticles3D _particles;

	// Camera stuff
	[Export] public Camera3D RevealCamera;
	[Export] public Marker3D[] CameraAngles;

	// Tuning
	[Export] public float LidImpulseUp    = 6f;
	[Export] public float LidImpulseHoriz = 1.5f;
	[Export] public float ShakeDuration   = 0.25f;
	[Export] public float ShakeMagnitude  = 3f;
	[Export] public float IdleShakePause  = 1.0f;
	[Export] public float LightCycleDuration = 2f;
	[Export] public float InitialStepDuration = 1.75f;

	private RandomNumberGenerator _rng = new();
	private Tween _hoverTween;
	private Tween _idleShakeTween;
	private Tween _lightCycleTween;
	private List<ItemResource> _rolledItems = new();
	private int _targetRarityIndex = 0;
	private Player _playerRef;

	private static readonly Dictionary<int, float> RarityWeights = new()
	{
		{ 1, 0.50f },
		{ 2, 0.25f },
		{ 3, 0.15f },
		{ 4, 0.08f },
		{ 5, 0.02f }
	};

	public override void _Ready()
	{
		base._Ready();

		if (RarityColors.Count() != RarityWeights.Count())
		{
			GD.PushError($"The amount of rarities and colors need to match in the treasure room scene: {Name}");
			return;
		}

		if (CameraAngles.Count() != RarityWeights.Count())
		{
			GD.PushError($"The amount of rarities and camera angles need to match in the treasure room scene: {Name}");
			return;
		}

		_rng.Randomize();
		UpdateLabel();
		_label.Visible = false;

		if (_openSoundPlayer != null && OpenSound != null)
		{
			_openSoundPlayer.Stream = OpenSound;
		}

		StartIdleShake();
		StartLightCycle();

		RollLoot();
		
		StandardMaterial3D particleMat = new();
		particleMat.AlbedoColor = RarityColors[_targetRarityIndex];
		particleMat.EmissionEnabled = true;
		particleMat.Emission = RarityColors[_targetRarityIndex];
		_particles.DrawPass1.SurfaceSetMaterial(0, particleMat);
	}

	private void StartLightCycle()
	{
		if (ChestLight == null){
			GD.PushWarning("TreasureChest: ChestLight is not assigned, skipping light cycle.");
			return;
		}
		_lightCycleTween?.Kill();
		_lightCycleTween = CreateTween().SetLoops();
		foreach (var color in RarityColors)
		{
			_lightCycleTween.TweenProperty(ChestLight, "light_color", color, LightCycleDuration).SetTrans(Tween.TransitionType.Sine).SetEase(Tween.EaseType.InOut);
		}
	}

	private void StartIdleShake()
	{
		_idleShakeTween?.Kill();
		float half = ShakeDuration * 0.25f;
		_idleShakeTween = CreateTween();
		_idleShakeTween.TweenProperty(this, "rotation_degrees:z",  ShakeMagnitude, half);
		_idleShakeTween.TweenProperty(this, "rotation_degrees:z", -ShakeMagnitude, half * 2f);
		_idleShakeTween.TweenProperty(this, "rotation_degrees:z",  0f,             half);
		_idleShakeTween.TweenCallback(Callable.From(() =>
		{
			if (!IsOpen)
				GetTree().CreateTimer(IdleShakePause).Timeout += () => { if (!IsOpen) StartIdleShake(); };
		}));
	}

    public override void Interact(Node caller)
	{
		OpenChest();
	}

	public void OpenChest()
	{
		if (IsOpen) 
		{
			return;
		}

		if (_playerRef == null)
		{
			_playerRef = GetTree().GetFirstNodeInGroup("player") as Player;
			if (_playerRef == null)
			{
				GD.PushError("Could not find player.");
				return;
			}
		}

		IsOpen = true;
		TaloTelemetry.TrackChestsOpened();

		_idleShakeTween?.Kill();
		_idleShakeTween = null;
		_lightCycleTween?.Kill();
		_lightCycleTween = null;
		_hoverTween?.Kill();
		_hoverTween = null;

		// Boost light energy on open
		if (ChestLight != null)
		{
			CreateTween().TweenProperty(ChestLight, "light_energy", ChestLight.LightEnergy * 2.5f, 0.3f).SetTrans(Tween.TransitionType.Quad).SetEase(Tween.EaseType.Out);
		}

		DoAnticipationShake(FinishOpen);
	}

	private void FinishOpen()
	{
		StartRarityReveal(_targetRarityIndex);

		LaunchLid();

		if (_particles != null)
		{
			_particles.Restart();
		}

		UpdateLabel();
		EmitSignal(SignalName.ChestOpened, this);
	}

	private void StartRarityReveal(int targetIndex)
	{
		// Switch to reveal carry
		RevealCamera.Current = true;

		// Set up tweens to cycle through rarity colors and cameras, pitch up sound each step
		var tween = CreateTween();
		float stepDuration = InitialStepDuration;

		for (int i = 0; i <= targetIndex; i++)
		{
			int idx = i; // Needed to sync up steps

			tween.TweenCallback(Callable.From(() =>
			{
				RevealCamera.Transform = CameraAngles[idx].Transform;
				RevealCamera.ResetPhysicsInterpolation();

				if (ChestLight != null)
					ChestLight.LightColor = RarityColors[idx];

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
				CreateTween().TweenProperty(ChestLight, "light_energy", 0f, 0.5f).SetTrans(Tween.TransitionType.Sine).SetEase(Tween.EaseType.In);
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
		tween.TweenProperty(this, "rotation_degrees:z", ShakeMagnitude, half);
		tween.TweenProperty(this, "rotation_degrees:z", -ShakeMagnitude, half * 2f);
		tween.TweenProperty(this, "rotation_degrees:z", 0f, half);
		tween.TweenCallback(Callable.From(() => onComplete?.Invoke()));
	}

	private void LaunchLid()
	{
		if (_lidBody == null) return;

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

	private void SpawnItems()
	{
		// Play sfx (placeholder)
		AudioManager.Instance.PlayGameVictorySound();

		if (ItemScene == null || LootPool.Count == 0)
		{
			GD.PushWarning($"TreasureChest: ItemScene={ItemScene != null}, LootPool.Count={LootPool.Count}");
			return;
		}

		for (int i = 0; i < _rolledItems.Count; i++)
		{
			var entry = _rolledItems[i];

			if (entry == null) { GD.PushWarning("TreasureChest: RollEntry returned null"); continue; }

			var item = ItemScene.Instantiate<Item>();
			if (item == null) { GD.PushWarning("TreasureChest: Instantiate<Item>() returned null — is the scene root typed as Item?"); continue; }

			item._resource = entry;
			item.TopLevel = true;
			GetParent().AddChild(item);

			float t = _rolledItems.Count > 1 ? i / (float)(_rolledItems.Count - 1) : 0.5f;
			float angle = Mathf.DegToRad(Mathf.Lerp(SpawnArcStart, SpawnArcEnd, t));
			item.GlobalPosition = GlobalPosition + new Vector3(
				Mathf.Sin(angle) * SpawnRadius,
				SpawnHeight,
				-Mathf.Cos(angle) * SpawnRadius
			);
		}

		Timer delay = new();
		delay.OneShot = true;
		delay.WaitTime = 2f;
		delay.Autostart = true;

		delay.Timeout += () =>
		{
			_playerRef.PlayerCamera.Current = true;
		};

		AddChild(delay);
	}

	private ItemResource RollEntry()
	{
		float roll = _rng.Randf();
		float cumulative = 0f;
		int rolledRarity = 1;

		foreach (var (rarity, weight) in RarityWeights.OrderByDescending(r => r.Key))
		{
			cumulative += weight;
			if (roll < cumulative) { rolledRarity = rarity; break; }
		}

		var candidates = LootPool.Where(e => e?.Rarity == rolledRarity).ToList();

		if (candidates.Count == 0)
			candidates = LootPool.OrderBy(e => System.Math.Abs(e.Rarity - rolledRarity)).ToList();

		return candidates.Count > 0 ? candidates[_rng.RandiRange(0, candidates.Count - 1)] : null;
	}

	
	private void RollLoot()
	{
		_rolledItems.Clear();

		int itemCount = _rng.RandiRange(MinItems, MaxItems);

		for (int i = 0; i < itemCount; i++)
		{
			var item = RollEntry();
			if (item != null)
				_rolledItems.Add(item);
		}

		_targetRarityIndex = _rolledItems.Count > 0
			? Mathf.Clamp(_rolledItems.Max(i => i.Rarity) - 1, 0, 4)
			: 0;
	}

	private void UpdateLabel()
	{
		if (_label == null) return;
		_label.Text = IsOpen ? "Treasure Chest (Empty)" : "Treasure Chest";
	}

    public override void HoverStart(Node caller)
	{
		_label.Visible = true;

		if (ChestLight == null || IsOpen)
		{
			return;
		}

		_hoverTween?.Kill();
		float baseEnergy = ChestLight.LightEnergy;
		_hoverTween = CreateTween().SetLoops();
		_hoverTween.TweenProperty(ChestLight, "light_energy", baseEnergy * 2f, 0.6f)
			.SetTrans(Tween.TransitionType.Sine).SetEase(Tween.EaseType.InOut);
		_hoverTween.TweenProperty(ChestLight, "light_energy", baseEnergy, 0.6f)
			.SetTrans(Tween.TransitionType.Sine).SetEase(Tween.EaseType.InOut);
	}

    public override void HoverEnd(Node caller)
    {
		_label.Visible = false;
		_hoverTween?.Kill();
		_hoverTween = null;
    }
}
