using Godot;
using System;

public partial class InteractableComponent : Node3D
{
	[Export]
	public bool interactable = true;
	[Signal]
	public delegate void InteractEventHandler(Node3D player);
	
	public bool interacted = false;
	private bool _playerInRange = false;
	private Node3D _player;
	private Area3D _area;
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		_area = GetNode<Area3D>("Area3D");
		_area.BodyEntered += OnBodyEntered;
		_area.BodyExited += OnBodyExited;
	}

	public override void _Input(InputEvent @event)
	{
		if (!_playerInRange || !interactable)// If either player not in range or connection are not enabled return
			return;

		if (@event.IsActionPressed("interact"))
		{
			interacted = true;
			EmitSignal(nameof(Interact),_player);
		}
	}
	
	private void OnBodyEntered(Node body)
	{
		if(body is Node3D node && body.Name == "Player")
		{
			_playerInRange = true;
			_player = node;
		}
	}

	private void OnBodyExited(Node body)
	{
		if (body == _player)
		{
			_playerInRange = false;
			_player = null;
			interacted = false;
		}
	}
	public override void _ExitTree()    
	{
		_area.BodyEntered -= OnBodyEntered;
		_area.BodyExited -= OnBodyExited;
		interacted = false;
	}
}
