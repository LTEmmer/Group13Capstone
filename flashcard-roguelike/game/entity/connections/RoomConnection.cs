using Godot;
using System;
using System.Numerics;
using Vector3 = Godot.Vector3;

public partial class RoomConnection : Node3D
{
	[Export] 
	public PackedScene[] ConnectionVisuals;
	[Export]
	public InteractableComponent Interactable;
	
	public bool PlayerInRoom = false;
	public int TargetRoomId { get; set; }
	public bool IsEntrance { get; set; }
	public bool connection_enabled = true; // New attribute to toggle connection availability
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
		
		Interactable.Interact += OnInteract;
		EventManager.Instance.listen("on_room_clear", new Callable(this, MethodName.on_room_clear));
	}

	private void OnInteract(Node3D player){
		TeleportPlayer(player);
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
		GetParent().GetParent().GetParent().RemoveChild(player);
		targetRoom.AddChild(player);
		player.GlobalPosition = dest;

		// Disable landing sound for next connection
		if (player is Player p)
		{
			p.SuppressNextLandSound = true;
		}

		if (CurrentRoomManager.Instance != null)
			CurrentRoomManager.Instance.CurrentRoomId = TargetRoomId;
	}

	public void SetLabel(bool isEntrance, int id, string roomType)
	{
		Label3D label = GetNodeOrNull<Label3D>("Label");
		if (label != null)
		{
			label.Text = isEntrance ? $"From Room {id} : {roomType} (In)" : $"To Room {id} : {roomType} (Out)";
		}
	}
	
	private void on_room_clear(string test){
		if(PlayerInRoom == true){
			this.Visible = true;
			this.connection_enabled = true;
		}
	}
}
