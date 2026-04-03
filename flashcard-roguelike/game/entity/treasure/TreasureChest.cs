using Godot;
using System.Collections.Generic;
using System.Linq;

public partial class TreasureChest : Node3D
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

	private bool _playerInRange = false;
	private Node3D _player;
	private Area3D _area;
	private MeshInstance3D _lidMesh;
	private Label3D _label;
	private RandomNumberGenerator _rng = new();

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
		_rng.Randomize();

		_area = GetNode<Area3D>("Area3D");
		_lidMesh = GetNodeOrNull<MeshInstance3D>("Lid");
		_label = GetNodeOrNull<Label3D>("Label3D");

		_area.BodyEntered += OnBodyEntered;
		_area.BodyExited  += OnBodyExited;

		UpdateLabel();
	}

	public override void _Input(InputEvent @event)
	{
		if (!_playerInRange || IsOpen) return;
		if (@event.IsActionPressed("interact")) OpenChest();
	}

	public void OpenChest()
	{
		if (IsOpen) return;
		IsOpen = true;

		if (_lidMesh != null)
		{
			var tween = CreateTween();
			tween.TweenProperty(_lidMesh, "rotation_degrees:x", -110f, 0.5f)
				.SetTrans(Tween.TransitionType.Bounce)
				.SetEase(Tween.EaseType.Out);
		}

		SpawnItems();
		UpdateLabel();
		EmitSignal(SignalName.ChestOpened, this);
	}

	private void SpawnItems()
	{
		if (ItemScene == null || LootPool.Count == 0)
		{
			GD.PushWarning($"TreasureChest: ItemScene={ItemScene != null}, LootPool.Count={LootPool.Count}");
			return;
		}

		int itemCount = _rng.RandiRange(MinItems, MaxItems);
		GD.Print($"TreasureChest: Attempting to spawn {itemCount} items");

		for (int i = 0; i < itemCount; i++)
		{
			var entry = RollEntry();
			if (entry == null) { GD.PushWarning("TreasureChest: RollEntry returned null"); continue; }

			var item = ItemScene.Instantiate<Item>();
			if (item == null) { GD.PushWarning("TreasureChest: Instantiate<Item>() returned null — is the scene root typed as Item?"); continue; }

			item.Resource = entry;
			item.TopLevel = true;
			GetParent().AddChild(item);

			float t = itemCount > 1 ? i / (float)(itemCount - 1) : 0.5f;
			float angle = Mathf.DegToRad(Mathf.Lerp(SpawnArcStart, SpawnArcEnd, t));
			item.GlobalPosition = GlobalPosition + new Vector3(
				Mathf.Sin(angle) * SpawnRadius,
				SpawnHeight,
				-Mathf.Cos(angle) * SpawnRadius
			);
		}
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

	private void OnBodyEntered(Node body)
	{
		if (body is CharacterBody3D && body.Name == "Player")
		{
			_playerInRange = true;
			_player = body as Node3D;
			UpdateLabel();
		}
	}

	private void OnBodyExited(Node body)
	{
		if (body == _player)
		{
			_playerInRange = false;
			_player = null;
			UpdateLabel();
		}
	}

	private void UpdateLabel()
	{
		if (_label == null) return;
		_label.Text = IsOpen         ? "Treasure Chest (Empty)"
					: _playerInRange ? "Treasure Chest\n[E] to open"
					: "Treasure Chest";
	}
}
