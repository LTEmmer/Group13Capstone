using Godot;
using System.Collections.Generic;
using System.Linq;

public partial class TreasureChest : Interactable
{
	[Export] public Godot.Collections.Array<ItemResource> LootPool = new();
	[Export] public PackedScene ItemScene;
	[Export] public int MinItems = 1;
	[Export] public int MaxItems = 3;
	[Export] public float SpawnRadius = 1.5f;
	[Export] public float SpawnHeight = 0.5f;
	[Export] public float SpawnArcStart = 90f;
	[Export] public float SpawnArcEnd = 260f;
	[Export] public bool IsOpen { get; private set; } = false;

	[Signal] public delegate void ChestOpenedEventHandler(TreasureChest chest);

	[Export] private MeshInstance3D _lidMesh;
	[Export] private Label3D _label;

	private RandomNumberGenerator _rng = new();

	private Node3D _sacrificeNode;
	private bool _resolved = false;

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

		if (LootPool == null || LootPool.Count == 0)
			LootPool = AllItems.Instance.ItemResources;

		_rng.Randomize();

		UpdateLabel();
		if (_label != null)
			_label.Visible = false;
	}

	public override void Interact(Node caller)
	{
		OpenChest();
	}

	public void OpenChest()
	{
		if (IsOpen) return;

		IsOpen = true;
		TaloTelemetry.TrackChestsOpened();

		if (_lidMesh != null)
		{
			var tween = CreateTween();
			tween.TweenProperty(_lidMesh, "rotation_degrees:x", -110f, 0.5f)
				.SetTrans(Tween.TransitionType.Bounce)
				.SetEase(Tween.EaseType.Out);
		}

		CreateSacrificeNode();
		SpawnItems();

		UpdateLabel();
		EmitSignal(SignalName.ChestOpened, this);
	}

	private void CreateSacrificeNode()
	{
		_sacrificeNode = new Node3D();
		_sacrificeNode.Name = "SacrificeNode";
		GetParent().AddChild(_sacrificeNode);
		_sacrificeNode.GlobalPosition = GlobalPosition;

		_sacrificeNode.ChildExitingTree += OnItemExiting;
	}

	private void OnItemExiting(Node node)
	{
		if (_resolved) return;
		if (node is not Item item) return;

		_resolved = true;

		GD.Print("Chest resolved: one item selected");

		// destroy everything else instantly
		if (_sacrificeNode != null && IsInstanceValid(_sacrificeNode))
		{
			_sacrificeNode.QueueFree();
			_sacrificeNode = null;
		}
	}

	private void SpawnItems()
	{
		if (ItemScene == null || LootPool.Count == 0)
		{
			GD.PushWarning("TreasureChest: Missing ItemScene or LootPool");
			return;
		}

		int itemCount = _rng.RandiRange(MinItems, MaxItems);

		for (int i = 0; i < itemCount; i++)
		{
			var entry = RollEntry();
			if (entry == null) continue;

			var item = ItemScene.Instantiate<Item>();
			if (item == null) continue;

			item._resource = entry;

			_sacrificeNode.AddChild(item);

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
			if (roll < cumulative)
			{
				rolledRarity = rarity;
				break;
			}
		}

		var candidates = LootPool.Where(e => e?.Rarity == rolledRarity).ToList();

		if (candidates.Count == 0)
			candidates = LootPool.OrderBy(e => System.Math.Abs(e.Rarity - rolledRarity)).ToList();

		return candidates.Count > 0
			? candidates[_rng.RandiRange(0, candidates.Count - 1)]
			: null;
	}

	private void UpdateLabel()
	{
		if (_label == null) return;

		_label.Text = IsOpen
			? "Treasure Chest (Empty)"
			: "Treasure Chest";
	}

	public override void HoverStart(Node caller)
	{
		if (_label != null)
			_label.Visible = true;
	}

	public override void HoverEnd(Node caller)
	{
		if (_label != null)
			_label.Visible = false;
	}
}