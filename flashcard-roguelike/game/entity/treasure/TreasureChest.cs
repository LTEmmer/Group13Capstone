using Godot;
using System;

/// <summary>
/// A treasure chest that can be opened to reveal items inside.
/// </summary>
public partial class TreasureChest : Node3D
{
	[Export]
	public PackedScene TreasureItemScene { get; set; }
	
	[Export]
	public int MinItems { get; set; } = 1;
	
	[Export]
	public int MaxItems { get; set; } = 3;
	
	[Export]
	public bool IsOpen { get; private set; } = false;
	
	[Signal]
	public delegate void ChestOpenedEventHandler(TreasureChest chest);
	
	private bool _playerInRange = false;
	private Node3D _player;
	private Area3D _area;
	private MeshInstance3D _lidMesh;
	private Label3D _label;
	private Node3D _itemSpawnPoint;
	private RandomNumberGenerator _rng = new RandomNumberGenerator();
	
	// Item name pools by rarity
	private static readonly string[] CommonItems = { "Gold Coin", "Silver Coin", "Bronze Coin", "Small Gem" };
	private static readonly string[] UncommonItems = { "Ruby", "Sapphire", "Emerald", "Ancient Coin" };
	private static readonly string[] RareItems = { "Diamond", "Golden Idol", "Magic Scroll", "Enchanted Ring" };
	private static readonly string[] EpicItems = { "Dragon Scale", "Phoenix Feather", "Mystic Orb", "Crown Fragment" };
	private static readonly string[] LegendaryItems = { "Legendary Artifact", "Ancient Relic", "Divine Treasure", "Cosmic Gem" };
	
	public override void _Ready()
	{
		_rng.Randomize();
		
		_area = GetNode<Area3D>("Area3D");
		_lidMesh = GetNodeOrNull<MeshInstance3D>("Lid");
		_label = GetNodeOrNull<Label3D>("Label3D");
		_itemSpawnPoint = GetNodeOrNull<Node3D>("ItemSpawnPoint");
		
		_area.BodyEntered += OnBodyEntered;
		_area.BodyExited += OnBodyExited;
		
		UpdateLabel();
	}
	
	public override void _Input(InputEvent @event)
	{
		if (!_playerInRange || IsOpen)
			return;

		if (@event.IsActionPressed("interact"))
		{
			OpenChest();
		}
	}
	
	/// <summary>
	/// Opens the chest and spawns treasure items.
	/// </summary>
	public void OpenChest()
	{
		if (IsOpen) return;
		
		IsOpen = true;
		GD.Print("Treasure chest opened!");
		
		// Animate lid opening
		if (_lidMesh != null)
		{
			Tween tween = CreateTween();
			tween.TweenProperty(_lidMesh, "rotation_degrees:x", -110f, 0.5f)
				.SetTrans(Tween.TransitionType.Bounce)
				.SetEase(Tween.EaseType.Out);
		}
		
		// Spawn items
		SpawnItems();
		
		// Update label
		UpdateLabel();
		
		EmitSignal(SignalName.ChestOpened, this);
	}
	
	private void SpawnItems()
	{
		if (TreasureItemScene == null)
		{
			GD.PushWarning("TreasureChest: No TreasureItemScene assigned!");
			return;
		}
		
		int itemCount = _rng.RandiRange(MinItems, MaxItems);
		Vector3 spawnBase = _itemSpawnPoint?.Position ?? new Vector3(0, 1.5f, 0);
		
		for (int i = 0; i < itemCount; i++)
		{
			TreasureItem item = TreasureItemScene.Instantiate() as TreasureItem;
			if (item == null) continue;
			
			// Determine rarity
			ItemRarity rarity = RollRarity();
			item.Rarity = rarity;
			item.ItemName = GetRandomItemName(rarity);
			item.Value = GetValueForRarity(rarity);
			item.ItemDescription = $"A {rarity.ToString().ToLower()} treasure worth {item.Value} gold.";
			
			// Position items in an arc above the chest
			float angle = (i / (float)itemCount) * Mathf.Pi - Mathf.Pi / 2f;
			float radius = 1.5f;
			Vector3 offset = new Vector3(
				Mathf.Cos(angle) * radius,
				0.5f + i * 0.3f,
				Mathf.Sin(angle) * radius * 0.5f
			);
			
			item.Position = spawnBase + offset;
			AddChild(item);
			
			GD.Print($"Spawned {item.ItemName} ({rarity})");
		}
	}
	
	private ItemRarity RollRarity()
	{
		float roll = _rng.Randf();
		
		// Rarity chances: Common 50%, Uncommon 25%, Rare 15%, Epic 8%, Legendary 2%
		if (roll < 0.02f) return ItemRarity.Legendary;
		if (roll < 0.10f) return ItemRarity.Epic;
		if (roll < 0.25f) return ItemRarity.Rare;
		if (roll < 0.50f) return ItemRarity.Uncommon;
		return ItemRarity.Common;
	}
	
	private string GetRandomItemName(ItemRarity rarity)
	{
		string[] pool = rarity switch
		{
			ItemRarity.Common => CommonItems,
			ItemRarity.Uncommon => UncommonItems,
			ItemRarity.Rare => RareItems,
			ItemRarity.Epic => EpicItems,
			ItemRarity.Legendary => LegendaryItems,
			_ => CommonItems
		};
		
		return pool[_rng.RandiRange(0, pool.Length - 1)];
	}
	
	private int GetValueForRarity(ItemRarity rarity)
	{
		return rarity switch
		{
			ItemRarity.Common => _rng.RandiRange(5, 15),
			ItemRarity.Uncommon => _rng.RandiRange(20, 50),
			ItemRarity.Rare => _rng.RandiRange(75, 150),
			ItemRarity.Epic => _rng.RandiRange(200, 400),
			ItemRarity.Legendary => _rng.RandiRange(500, 1000),
			_ => 10
		};
	}
	
	private void UpdateLabel()
	{
		if (_label == null) return;
		
		if (IsOpen)
		{
			_label.Text = "Treasure Chest (Empty)";
		}
		else if (_playerInRange)
		{
			_label.Text = "Treasure Chest\n[E] to open";
		}
		else
		{
			_label.Text = "Treasure Chest";
		}
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
}
