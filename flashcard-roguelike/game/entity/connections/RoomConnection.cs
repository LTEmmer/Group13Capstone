using Godot;
using System;
using System.Numerics;
using Vector3 = Godot.Vector3;

public partial class RoomConnection : Node3D
{
    [Export] public PackedScene[] ConnectionVisuals;
    public int TargetRoomId { get; set; }
    public bool IsEntrance { get; set; }
    private bool _playerInRange = false;
    private Node3D _player;
    private Area3D _area;

    public override void _Ready()
    {
        // Spawn random visual
        if (ConnectionVisuals == null || ConnectionVisuals.Length == 0)
        {
            GD.PushWarning("RoomConnection has no ConnectionVisuals assigned. No visual representation will be created.");
            return;
        }
        
        RandomNumberGenerator rng = new RandomNumberGenerator();
        rng.Randomize();
        PackedScene scene = ConnectionVisuals[rng.RandiRange(0, ConnectionVisuals.Length - 1)];
        AddChild(scene.Instantiate());
    
        _area = GetNode<Area3D>("Area3D");
        _area.BodyEntered += OnBodyEntered;
        _area.BodyExited += OnBodyExited;
    }

    public override void _Input(InputEvent @event)
    {
        if (!_playerInRange)
            return;

        if (@event.IsActionPressed("interact"))
        {
            TeleportPlayer(_player);
        }
    }

    public override void _ExitTree()    
    {
        _area.BodyEntered -= OnBodyEntered;
        _area.BodyExited -= OnBodyExited;
    }

    private void OnBodyEntered(Node body)
    {
        if (body is Node3D node && body.Name == "Player")
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
        }
    }

    public void TeleportPlayer(Node3D player)
    {
        DungeonGenerator gen = GetTree().Root.GetNodeOrNull<DungeonGenerator>("Dungeon");
        if (gen == null)        
        {
            GD.PushError("RoomConnection could not find DungeonGenerator in the scene tree. Teleportation failed.");
            return;
        }

        Node3D targetRoom = gen.GetRoomNode(TargetRoomId);
        if (targetRoom == null)
        {
            GD.PushError($"RoomConnection could not find target room with ID {TargetRoomId}. Teleportation failed.");
            return;
        }

        Vector3 dest = Vector3.Zero;
        if (IsEntrance)
        {
            dest = (Vector3)targetRoom.GetNodeOrNull<Node3D>("ExitPoint")?.GlobalPosition;
        }
        else
        {
            dest = (Vector3)targetRoom.GetNodeOrNull<Node3D>("EnterPoint")?.GlobalPosition;
        }

        if (dest == Vector3.Zero)
        {
            GD.PushError($"RoomConnection could not find an 'EnterPoint' or 'ExitPoint' in the target room {TargetRoomId}. Teleportation failed.");
            return;
        }

        player.GlobalPosition = dest;
    }

    public void SetLabel(bool isEntrance, int id, string roomType)
    {
        Label3D label = GetNodeOrNull<Label3D>("Label");
        if (label != null)
        {
            label.Text = isEntrance ? $"From Room {id} : {roomType} (In)" : $"To Room {id} : {roomType} (Out)";
        }
    }
}