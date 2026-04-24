using Godot;
using System.Collections.Generic;
using System.Linq;

public partial class TreasureChest : Interactable
{
	[Export] public PackedScene ItemScene;
	[Export] public int MinItems = 1;
	[Export] public int MaxItems = 3;
	[Export] public float SpawnRadius = 1.5f;
	[Export] public float SpawnHeight = 0.5f;
	[Export] public float SpawnArcStart = 90f;
	[Export] public float SpawnArcEnd = 260f;
	[Export] public bool IsOpen { get; private set; } = false;

	[Signal] public delegate void ChestOpenedEventHandler(TreasureChest chest);

	[Export] private Label3D _label;
	[Export] private RigidBody3D _lidBody;

	[Export] public AudioStream OpenSound;
	[Export] public AudioStream ItemSpawnSound;
	[Export] private AudioStreamPlayer3D _openSoundPlayer;

	private RandomNumberGenerator _rng = new();
	private List<ItemResource> _rolledItems = new();
	private int _targetRarityIndex = 0;
	private Player _playerRef;

	private Node3D _sacrificeNode;
	private bool _resolved = false;
	private Node3D _shakeNode;

	public override void _Ready()
	{
		base._Ready();

		if (RarityColors.Length != CameraAngles.Length)
		{
			GD.PushError($"RarityColors count ({RarityColors.Length}) must match CameraAngles count ({CameraAngles.Length}) in {Name}");
			return;
		}

		_lidBody.Freeze = true;

		_rng.Randomize();
		UpdateLabel();
		_label.Visible = false;

		if (_openSoundPlayer != null && OpenSound != null)
		{
			_openSoundPlayer.Stream = OpenSound;
		}

		_shakeNode = GetNodeOrNull<Node3D>("ShakeNode");

		StartIdleShake();
		StartLightCycle();

		var items = AllItemsManager.Instance.GetRandomItems(_rng.RandiRange(MinItems, MaxItems), allowDuplicates: false);
		if (items != null)
			_rolledItems.AddRange(items);

		_targetRarityIndex = _rolledItems.Count > 0
			? Mathf.Clamp(_rolledItems.Max(i => i.Rarity) - 1, 0, RarityColors.Length - 1)
			: 0;

		StandardMaterial3D particleMat = new();
		particleMat.AlbedoColor = RarityColors[_targetRarityIndex];
		particleMat.EmissionEnabled = true;
		particleMat.Emission = RarityColors[_targetRarityIndex];
		_particles.DrawPass1.SurfaceSetMaterial(0, particleMat);
	}

	public override void Interact(Node caller) => OpenChest();

	public void OpenChest()
	{
		if (IsOpen) return;

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

	private void SpawnItems()
	{
		AudioManager.Instance.PlayGameVictorySound();

		if (ItemScene == null || _rolledItems.Count == 0)
		{
			GD.PushWarning($"TreasureChest: ItemScene={ItemScene != null}, _rolledItems.Count={_rolledItems.Count}");
			return;
		}

		_sacrificeNode = new Node3D { Name = "SacrificeNode" };
		GetParent().AddChild(_sacrificeNode);
		_sacrificeNode.GlobalPosition = GlobalPosition;
		_sacrificeNode.ChildExitingTree += OnItemExiting;

		var tween = CreateTween();

		for (int i = 0; i < _rolledItems.Count; i++)
		{
			int idx = i;
			tween.TweenCallback(Callable.From(() => SpawnSingleItem(idx)));
			if (i < _rolledItems.Count - 1)
			{
				tween.TweenInterval(1f);
			}
		}

		tween.TweenInterval(2f);
		tween.TweenCallback(Callable.From(() => { _playerRef.PlayerCamera.Current = true; }));
	}

	private void SpawnSingleItem(int i)
	{
		if (!IsInsideTree() || _resolved) return;

		var entry = _rolledItems[i];
		if (entry == null)
		{
			GD.PushWarning("TreasureChest: rolled item is null"); return;
		}

		var item = ItemScene.Instantiate<Item>();
		if (item == null)
		{
			GD.PushWarning("TreasureChest: Instantiate<Item>() returned null — is the scene root typed as Item?"); return;
		}

		item._resource = entry;
		_sacrificeNode.AddChild(item);

		float t = _rolledItems.Count > 1 ? i / (float)(_rolledItems.Count - 1) : 0.5f;
		float angle = Mathf.DegToRad(Mathf.Lerp(SpawnArcStart, SpawnArcEnd, t));
		Vector3 finalPos = GlobalPosition + new Vector3(
			Mathf.Sin(angle) * SpawnRadius,
			SpawnHeight,
			-Mathf.Cos(angle) * SpawnRadius
		);

		// Launch from chest opening with ballistic arc
		Vector3 startPos = GlobalPosition + Vector3.Up * 0.5f;
		item.GlobalPosition = startPos;

		Vector3 peak = (startPos + finalPos) * 0.5f;
		peak.Y = Mathf.Max(startPos.Y, finalPos.Y) + 1.5f;

		var launchTween = item.CreateTween();
		launchTween.TweenProperty(item, "global_position", peak, 0.3f).SetTrans(Tween.TransitionType.Quad).SetEase(Tween.EaseType.Out);
		launchTween.TweenProperty(item, "global_position", finalPos, 0.3f).SetTrans(Tween.TransitionType.Quad).SetEase(Tween.EaseType.In);

		if (ItemSpawnSound != null && _openSoundPlayer != null)
		{
			_openSoundPlayer.Stream = ItemSpawnSound;
			_openSoundPlayer.PitchScale = 1f + (float)GD.RandRange(-.25f, .25f);
			_openSoundPlayer.Play();
		}
	}

	private void OnItemExiting(Node node)
	{
		if (_resolved || node is not Item) return;

		_resolved = true;
		GD.Print("Chest resolved: one item selected");

		if (_sacrificeNode != null && IsInstanceValid(_sacrificeNode))
		{
			_sacrificeNode.QueueFree();
			_sacrificeNode = null;
		}
	}

	private void UpdateLabel()
	{
		if (_label == null) return;
		_label.Text = IsOpen ? "Treasure Chest (Empty)" : "Treasure Chest";
	}

	public override void HoverStart(Node caller)
	{
		_label.Visible = true;

		if (ChestLight == null || IsOpen) return;

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

	public void SetCollision(bool enabled)
	{
		Set("collision_layer", enabled ? 1 : 0);
		_lidBody.CollisionLayer = enabled ? (uint)1 : (uint)0;
		Visible = enabled;
	}
}
