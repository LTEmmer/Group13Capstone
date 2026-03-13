using System;
using Godot;

public partial class BaseNPC : Node3D
{
    [Signal]
    public delegate void OnInteractionEventHandler();

    private Area3D _area;
    private bool _playerInRange = false;
    private bool _interactionTriggered = false;

    public override void _Ready()
    {
        _area = GetNode<Area3D>("Area3D");
        _area.BodyEntered += OnBodyEntered;
        _area.BodyExited += OnBodyExited;
    }

    public override void _Input(InputEvent @event)
	{
		if (@event.IsActionPressed("interact") && _playerInRange && _interactionTriggered == false)
		{
			EmitSignal(nameof(OnInteraction));
			_interactionTriggered = true;
		}
	}

    private void OnBodyEntered(Node body)
    {
        if (body is Node3D node && body.Name == "Player")
        {
            _playerInRange = true;
        }
    }

    private void OnBodyExited(Node body)
    {
        if (body is Node3D node && node.Name == "Player")
        {
            _playerInRange = false;
        }
    }
}