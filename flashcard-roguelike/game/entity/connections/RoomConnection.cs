using Godot;
using System;
using System.Numerics;
using Vector3 = Godot.Vector3;

public partial class RoomConnection : Interactable
{
	[Export]
	public PackedScene[] ConnectionVisuals; // Fallback visuals (used when no type-specific array is assigned)
	[Export]
	public PackedScene[] EntranceVisuals;
	[Export]
	public PackedScene[] CombatVisuals;
	[Export]
	public PackedScene[] EventVisuals;
	[Export]
	public PackedScene[] TreasureVisuals;
	[Export]
	public PackedScene[] ExitVisuals;

	public bool PlayerInRoom = false;
	public int TargetRoomId { get; set; }
	public RoomTypes TargetRoomType { get; set; }
	public bool IsEntrance { get; set; }
	public bool connection_enabled = true; // New attribute to toggle connection availability
	private bool _playerInRange = false;
	private Node3D _player;
	private Area3D _area;


	public override void _Ready()
	{
		base._Ready();
		// Pick the visual array for the target room type, falling back to ConnectionVisuals
		PackedScene[] visuals = TargetRoomType switch
		{
			RoomTypes.Entrance => EntranceVisuals?.Length > 0 ? EntranceVisuals : ConnectionVisuals,
			RoomTypes.Combat   => CombatVisuals?.Length   > 0 ? CombatVisuals   : ConnectionVisuals,
			RoomTypes.Event    => EventVisuals?.Length    > 0 ? EventVisuals    : ConnectionVisuals,
			RoomTypes.Treasure => TreasureVisuals?.Length > 0 ? TreasureVisuals : ConnectionVisuals,
			RoomTypes.Exit     => ExitVisuals?.Length     > 0 ? ExitVisuals     : ConnectionVisuals,
			_                  => ConnectionVisuals,
		};

		if (visuals == null || visuals.Length == 0)
		{
			GD.PushWarning("RoomConnection has no ConnectionVisuals assigned. No visual representation will be created.");
			return;
		}

		RandomNumberGenerator rng = new RandomNumberGenerator();
		rng.Randomize();
		PackedScene scene = visuals[rng.RandiRange(0, visuals.Length - 1)];
		AddChild(scene.Instantiate());
		
		EventManager.Instance.listen("on_room_clear", new Callable(this, MethodName.on_room_clear));
	}

	public override void Interact(Node caller)
	{
		OnInteract(caller as Node3D);
	}


	private void OnInteract(Node3D player){
		TeleportPlayer(player);
	}

	public void TeleportPlayer(Node3D player)
	{
		DungeonGenerator gen = GetTree().Root.GetNodeOrNull<DungeonGenerator>("DungeonGenerator");
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

		Transform3D dest = Transform3D.Identity;
		if (IsEntrance)
		{
			dest = targetRoom.GetNodeOrNull<Node3D>("ExitPoint")?.GlobalTransform ?? Transform3D.Identity;
		}
		else
		{
			dest = targetRoom.GetNodeOrNull<Node3D>("EnterPoint")?.GlobalTransform ?? Transform3D.Identity;
		}

		if (dest == Transform3D.Identity)
		{
			GD.PushError($"RoomConnection could not find an 'EnterPoint' or 'ExitPoint' in the target room {TargetRoomId}. Teleportation failed.");
			return;
		}
		GetParent().GetParent().GetParent().RemoveChild(player);
		targetRoom.AddChild(player);
		player.GlobalTransform = dest;
		TaloTelemetry.TrackFloorsCleared();

		// Disable landing sound for next connection
		if (player is Player p)
		{
			p.SuppressNextLandSound = true;
		}

		if (CurrentRoomManager.Instance != null)
			CurrentRoomManager.Instance.CurrentRoomId = TargetRoomId;
	}

	public void SetLabel(string roomType, bool onMainPath)
	{
		Label3D label = GetNodeOrNull<Label3D>("Label");
		if (label != null)
		{
			label.Text = roomType;

			// Set label to white if on main path, green if on branch
			label.Modulate = onMainPath ? new Color(255, 255, 255, 1f) : new Color(0, 119, 0, 1f);
		}
	}
	
	private void on_room_clear(string test){
		if(PlayerInRoom == true){
			this.Visible = true;
			this.connection_enabled = true;
		}
	}
}
