using Godot;
using System;

/// <summary>
/// Item rarity levels that affect appearance and value.
/// </summary>
public enum ItemRarity
{
	Common,
	Uncommon,
	Rare,
	Epic,
	Legendary
}

/// <summary>
/// Represents a collectible treasure item that the player can pick up.
/// </summary>
public partial class TreasureItem : Node3D
{
	[Export]
	public string ItemName { get; set; } = "Treasure";
	
	[Export]
	public string ItemDescription { get; set; } = "A valuable treasure.";
	
	[Export]
	public int Value { get; set; } = 10;
	
	[Export]
	public ItemRarity Rarity { get; set; } = ItemRarity.Common;
	
	[Signal]
	public delegate void ItemCollectedEventHandler(TreasureItem item);
	
	private bool _playerInRange = false;
	private Node3D _player;
	private Area3D _area;
	private MeshInstance3D _mesh;
	private Label3D _label;
	private float _bobTime = 0f;
	private Vector3 _initialPosition;
	
	public override void _Ready()
	{
		_area = GetNode<Area3D>("Area3D");
		_mesh = GetNodeOrNull<MeshInstance3D>("MeshInstance3D");
		_label = GetNodeOrNull<Label3D>("Label3D");
		
		_area.BodyEntered += OnBodyEntered;
		_area.BodyExited += OnBodyExited;
		
		_initialPosition = Position;
		
		// Update label with item name
		if (_label != null)
		{
			_label.Text = ItemName;
		}
		
		// Apply rarity color to mesh
		ApplyRarityColor();
	}
	
	public override void _Process(double delta)
	{
		// Bobbing animation
		_bobTime += (float)delta * 2f;
		Position = _initialPosition + new Vector3(0, Mathf.Sin(_bobTime) * 0.15f, 0);
		
		// Rotate slowly
		if (_mesh != null)
		{
			_mesh.RotateY((float)delta * 1.5f);
		}
	}
	
	public override void _Input(InputEvent @event)
	{
		if (!_playerInRange)
			return;

		if (@event.IsActionPressed("interact"))
		{
			Collect();
		}
	}
	
	/// <summary>
	/// Collects the treasure item and removes it from the scene.
	/// </summary>
	public void Collect()
	{
		GD.Print($"Collected {ItemName} (Value: {Value}, Rarity: {Rarity})");
		EmitSignal(SignalName.ItemCollected, this);
		QueueFree();
	}
	
	private void ApplyRarityColor()
	{
		if (_mesh == null) return;
		
		StandardMaterial3D material = new StandardMaterial3D();
		
		switch (Rarity)
		{
			case ItemRarity.Common:
				material.AlbedoColor = new Color(0.8f, 0.8f, 0.8f); // Gray
				break;
			case ItemRarity.Uncommon:
				material.AlbedoColor = new Color(0.2f, 0.8f, 0.2f); // Green
				break;
			case ItemRarity.Rare:
				material.AlbedoColor = new Color(0.2f, 0.4f, 1.0f); // Blue
				break;
			case ItemRarity.Epic:
				material.AlbedoColor = new Color(0.6f, 0.2f, 0.8f); // Purple
				break;
			case ItemRarity.Legendary:
				material.AlbedoColor = new Color(1.0f, 0.8f, 0.0f); // Gold
				material.Emission = new Color(1.0f, 0.8f, 0.0f);
				material.EmissionEnergyMultiplier = 0.5f;
				break;
		}
		
		_mesh.MaterialOverride = material;
	}
	
	private void OnBodyEntered(Node body)
	{
		if (body is CharacterBody3D && body.Name == "Player")
		{
			_playerInRange = true;
			_player = body as Node3D;
			
			// Show interaction hint
			if (_label != null)
			{
				_label.Text = $"{ItemName}\n[E] to collect";
			}
		}
	}
	
	private void OnBodyExited(Node body)
	{
		if (body == _player)
		{
			_playerInRange = false;
			_player = null;
			
			// Hide interaction hint
			if (_label != null)
			{
				_label.Text = ItemName;
			}
		}
	}
}
