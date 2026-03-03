using Godot;
using System;
using System.Collections.Generic;

/// <summary>
/// Represents item data for treasure generation.
/// </summary>
public class ItemData
{
	public string Name { get; set; }
	public int Value { get; set; }
	public string Description { get; set; }
	
	public ItemData(string name, int value, string description)
	{
		Name = name;
		Value = value;
		Description = description;
	}
}

/// <summary>
/// Represents rarity tier configuration with chance and available items.
/// </summary>
public class RarityTier
{
	public float Chance { get; set; }
	public List<ItemData> Items { get; set; }
	
	public RarityTier(float chance, List<ItemData> items)
	{
		Chance = chance;
		Items = items;
	}
}

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
	
	// Item dictionary organized by rarity with chances and item pools
	private static readonly Dictionary<ItemRarity, RarityTier> ItemDict = new()
	{
		{
			ItemRarity.Common, new RarityTier(0.50f, new List<ItemData>
			{
				new("Gold Coin", 10, "A shiny gold coin"),
				new("Silver Coin", 8, "A polished silver coin"),
				new("Bronze Coin", 5, "An old bronze coin"),
				new("Small Gem", 15, "A small uncut gem")
			})
		},
		{
			ItemRarity.Uncommon, new RarityTier(0.25f, new List<ItemData>
			{
				new("Ruby", 35, "A deep red ruby"),
				new("Sapphire", 40, "A brilliant blue sapphire"),
				new("Emerald", 45, "A vivid green emerald"),
				new("Ancient Coin", 50, "A coin from a forgotten era")
			})
		},
		{
			ItemRarity.Rare, new RarityTier(0.15f, new List<ItemData>
			{
				new("Diamond", 100, "A flawless diamond"),
				new("Golden Idol", 125, "An idol of pure gold"),
				new("Magic Scroll", 150, "A scroll imbued with magic"),
				new("Enchanted Ring", 140, "A ring with mystical properties")
			})
		},
		{
			ItemRarity.Epic, new RarityTier(0.08f, new List<ItemData>
			{
				new("Dragon Scale", 300, "A scale from an ancient dragon"),
				new("Phoenix Feather", 350, "A feather that glows with inner fire"),
				new("Mystic Orb", 400, "An orb pulsing with arcane energy"),
				new("Crown Fragment", 375, "A piece of a legendary crown")
			})
		},
		{
			ItemRarity.Legendary, new RarityTier(0.02f, new List<ItemData>
			{
				new("Legendary Artifact", 750, "An artifact of immense power"),
				new("Ancient Relic", 850, "A relic from the dawn of time"),
				new("Divine Treasure", 1000, "A treasure blessed by the gods"),
				new("Cosmic Gem", 900, "A gem containing starlight")
			})
		}
	};
	
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
			
			// Determine rarity and get random item from dictionary
			ItemRarity rarity = RollRarity();
			ItemData itemData = GetRandomItem(rarity);
			
			item.Rarity = rarity;
			item.ItemName = itemData.Name;
			item.Value = itemData.Value;
			item.ItemDescription = itemData.Description;
			
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
		float cumulative = 0f;
		
		// Roll through rarities from rarest to most common using dictionary chances
		foreach (var rarity in new[] { ItemRarity.Legendary, ItemRarity.Epic, ItemRarity.Rare, ItemRarity.Uncommon, ItemRarity.Common })
		{
			cumulative += ItemDict[rarity].Chance;
			if (roll < cumulative)
				return rarity;
		}
		
		return ItemRarity.Common;
	}
	
	private ItemData GetRandomItem(ItemRarity rarity)
	{
		var items = ItemDict[rarity].Items;
		return items[_rng.RandiRange(0, items.Count - 1)];
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
}
