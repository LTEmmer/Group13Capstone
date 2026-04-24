using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class DungeonGenerator : Node3D
{
	[Export] public int MaxRoomCount = 8;
	[Export] public int MinCombatRooms = 1;
	[Export] public int MinEventRooms = 1;
	[Export] public int MinTreasureRooms = 1;
	[Export] public float BranchChance = 0.5f; // Per-eligible-room chance to sprout a branch
	[Export] public int MaxBranchLength = 2; // Max rooms deep per branch; the last room is always treasure
	[Export] public bool UseRandomSeed = true;
	[Export] public int Seed = 1234;
	[Export] public RoomConfig EntranceConfig;
	[Export] public RoomConfig ExitConfig;
	[Export] public RoomConfig[] CombatConfigs;
	[Export] public RoomConfig[] EventConfigs;
	[Export] public RoomConfig[] TreasureConfigs;
	[Export] public PackedScene ConnectionScene;
	[Export] public PackedScene PlayerScene;

	private Player _player;
	private readonly RandomNumberGenerator _rng = new RandomNumberGenerator();
	private DungeonGraph _graph;

	public DungeonGraph Graph => _graph;

	public override void _Ready()
	{
		AddToGroup("dungeon_generator");
		TaloTelemetry.ResetSessionStats();
		AudioManager.Instance?.PlayDungeonMusic();
		RegenerateDungeon();
	}

	// Called by VictoryMenu to advance to the next floor without reloading the scene.
	// Clears the current dungeon, bumps difficulty/floor, then regenerates in place.
	public void GoToNextFloor()
	{
		GameDifficultyManager.Instance?.AdvanceFloor();

		// Detach player before clearing rooms so it isn't freed along with them
		_player?.GetParent()?.RemoveChild(_player);

		Node3D roomsRoot = GetOrCreateRoot("Rooms");
		ClearChildren(roomsRoot);
		RegenerateDungeon();
		AudioManager.Instance?.PlayDungeonMusic();
	}

	private void RegenerateDungeon()
	{
		// Initialize random number generator with seed
		if (UseRandomSeed)
		{
			_rng.Randomize();
		}
		else
		{
			_rng.Seed = (ulong)Seed;
		}

		_graph = GenerateGraph(); // Generate the dungeon graph structure
		_graph.PrintGraph();
		Dictionary<int, Vector3> positions = GenerateLayout(_graph); // Generate world positions for rooms
		SpawnRooms(_graph, positions); // Instantiate room scenes and connections based on the graph and layout
		SpawnPlayer(); // Place the player at the entrance room's EnterPoint

		if (CurrentRoomManager.Instance != null)
		{
			CurrentRoomManager.Instance.CurrentRoomId = 0;
			CurrentRoomManager.Instance.GraphRef = _graph; // Expose graph so minimap and other systems can read room data
		}

		SceneTransition.FadeIn(this);
	}


	public DungeonGraph GenerateGraph()
	{
		// Ensure that MaxRoomCount is sufficient to accommodate the minimum required rooms
		int minRoomCount = MinCombatRooms + MinEventRooms + MinTreasureRooms + 2; // +2 for entrance and exit
		if (MaxRoomCount < minRoomCount)
		{
			GD.PushError($"MaxRoomCount must be at least {minRoomCount} to accommodate the minimum required rooms. Adjusting MaxRoomCount to {minRoomCount}.");
			MaxRoomCount = minRoomCount;
		}
		int roomCount = _rng.RandiRange(minRoomCount, MaxRoomCount);

		// Phase 1: Build ordered config list — entrance first, middle rooms (shuffled), exit last
		List<RoomConfig> configs = new List<RoomConfig>();
		configs.Add(EntranceConfig); // Added first to ensure it's at the start

		for (int i = 0; i < MinCombatRooms; i++) // Add minimum required combat rooms
		{
			configs.Add(CombatConfigs[_rng.RandiRange(0, CombatConfigs.Length - 1)]);
		}

		for (int i = 0; i < MinEventRooms; i++) // Add minimum required event rooms
		{
			configs.Add(EventConfigs[_rng.RandiRange(0, EventConfigs.Length - 1)]);
		}

		for (int i = 0; i < MinTreasureRooms; i++) // Add minimum required treasure rooms
		{
			configs.Add(TreasureConfigs[_rng.RandiRange(0, TreasureConfigs.Length - 1)]);
		}

		// Fill remaining slots with random room types up to the rolled room count
		RoomConfig[][] pools = {CombatConfigs, EventConfigs, TreasureConfigs};
		int remaining = roomCount - 2 - (MinCombatRooms + MinEventRooms + MinTreasureRooms);
		for (int i = 0; i < remaining; i++)
		{
			RoomConfig[] pool = pools[_rng.RandiRange(0, pools.Length - 1)];
			configs.Add(pool[_rng.RandiRange(0, pool.Length - 1)]);
		}

		// Shuffle middle room configs (index 1 to end, entrance stays at 0)
		for (int i = configs.Count - 1; i > 1; i--)
		{
			int j = _rng.RandiRange(1, i);
			(configs[i], configs[j]) = (configs[j], configs[i]);
		}

		configs.Add(ExitConfig); // Added last to ensure it's at the end

		// Phase 2: Build graph and connect rooms linearly: entrance -> middle rooms -> exit.
		// Connecting in order guarantees every main-path room is reachable with no retries or deadlocks.
		_graph = new DungeonGraph(configs);
		for (int i = 0; i < configs.Count - 1; i++)
		{
			_graph.TryConnect(i, i + 1);
		}

		// Assign main-path metadata and minimap grid positions (row 0 = main path)
		for (int i = 0; i < _graph.Rooms.Count; i++)
		{
			_graph.Rooms[i].IsOnMainPath = true;
			_graph.Rooms[i].MinimapGridPosition = new Vector2I(i, 0);
		}

		// Phase 3: Sprout dead-end branches from eligible combat rooms.
		// Branches always end with a treasure room to reward exploration.
		// TryConnect silently rejects invalid connections, so no deadlock risk.
		int mainPathCount = _graph.Rooms.Count;
		int branchRow = 1; // Alternates between row +1 and -1 so branches don't overlap on the minimap
		for (int i = 1; i < mainPathCount - 1; i++)
		{
			DungeonRoom room = _graph.Rooms[i];
			if (!room.Config.AllowBranch) 
			{
				continue; // Only rooms configured to allow branching (e.g. combat rooms)
			}

			if (_rng.Randf() >= BranchChance) 
			{
				continue; // Roll per room
			}

			if (room.OutgoingConnections.Count >= room.MaxConnections) 
			{
				continue; // Need a free outgoing slot
			}

			int branchDepth = _rng.RandiRange(1, Math.Max(1, MaxBranchLength));
			int parentCol = room.MinimapGridPosition.X;
			int branchDir = (branchRow % 2 == 0) ? 1 : -1; // +1 = above, -1 = below the main path on the minimap
			int prevId = room.Id;

			for (int d = 0; d < branchDepth; d++)
			{
				bool isLast = (d == branchDepth - 1);
				RoomConfig cfg = isLast ? PickTreasureConfig() : PickMidBranchConfig();
				int newId = _graph.AddBranchRoom(cfg, _graph.Rooms[prevId].Depth + 1);
				_graph.Rooms[newId].IsOnMainPath = false;

				// Each branch room is at the same column as its parent, stacking vertically
				_graph.Rooms[newId].MinimapGridPosition = new Vector2I(parentCol, branchDir * (d + 1));
				if (!_graph.TryConnect(prevId, newId))
				{
					GD.PushWarning($"Branch connection from {prevId} to {newId} was rejected — skipping remaining branch depth.");
					break;
				}
				prevId = newId;
			}
			branchRow++;
		}

		// Check if all rooms are reachable from the entrance and print a warning if not.
		if (!_graph.AreAllRoomsReachable())
		{
			GD.PushWarning("Not all rooms are reachable in the generated dungeon graph.");
		}
		else
		{
			GD.Print("All rooms are reachable in the generated dungeon graph.");
		}

		return _graph;
	}

	// Picks a random treasure config for the end of a branch
	private RoomConfig PickTreasureConfig() => TreasureConfigs[_rng.RandiRange(0, TreasureConfigs.Length - 1)];

	// Picks a random non-treasure config for intermediate branch rooms
	private RoomConfig PickMidBranchConfig()
	{
		RoomConfig[][] pools = { CombatConfigs, EventConfigs };
		RoomConfig[] pool = pools[_rng.RandiRange(0, pools.Length - 1)];
		return pool[_rng.RandiRange(0, pool.Length - 1)];
	}

	public Node3D GetRoomNode(int roomId)
	{
		return GetNodeOrNull<Node3D>($"Rooms/Room_{roomId}");
	}

	public string GetRoomDisplayName(int roomId)
	{
		Node3D roomNode = GetRoomNode(roomId);
		if (roomNode == null)
		{
			return "Unknown";
		}

		if (!roomNode.HasMeta("RoomId") || !roomNode.HasMeta("RoomType"))
		{
			return $"Room {roomId}";
		}

		int id = (int)roomNode.GetMeta("RoomId");
		string type = (string)roomNode.GetMeta("RoomType");
		return $"Room {id} ({type})";
	}

	private Dictionary<int, Vector3> GenerateLayout(DungeonGraph graph)
	{
		// Rooms are placed in a grid based on their minimap positions to avoid overlapping.
		// Main-path rooms sit along the x-axis (z=0); branch rooms are offset on the z-axis.
		Dictionary<int, Vector3> positions = new Dictionary<int, Vector3>(graph.Rooms.Count);
		foreach (DungeonRoom room in graph.Rooms)
		{
			float x = room.MinimapGridPosition.X * 300f;
			float z = room.MinimapGridPosition.Y * 300f; // Row ±1 shifts branch rooms 300 units on Z
			positions[room.Id] = new Vector3(x, 0, z);
		}
		return positions;
	}

	private void SpawnRooms(DungeonGraph graph, Dictionary<int, Vector3> positions)
	{
		// Get Rooms node and clear existing rooms for regeneration.
		Node3D roomsRoot = GetOrCreateRoot("Rooms");
		ClearChildren(roomsRoot);

		// Instantiate each room and its connections based on the graph structure and the generated layout.
		foreach (DungeonRoom room in graph.Rooms)
		{
			// Create a new room node and set its position
			Node3D roomNode = CreateRoomNode(room.Id, room.Config);
			roomNode.Position = positions[room.Id];

			// Get the entrances and exits nodes and check for null
			Node3D entrances = roomNode.GetNodeOrNull<Node3D>("Entrances");
			Node3D exits = roomNode.GetNodeOrNull<Node3D>("Exits");

			int entranceIndex = 0;
			int exitIndex = 0;

			if (entrances == null || exits == null)
			{
				GD.PushWarning($"Room {room.Id} is missing an 'Exits' or 'Entrances' Node3D child. Connections will not be properly aligned.");
				continue;
			}

			// Instantiate incoming connections for the room
			foreach (int incoming in room.IncomingConnections)
			{
				bool isBranch = graph.GetRoom(incoming).IsOnMainPath;
				PackedScene conn = ConnectionScene;
				RoomConnection connInstance = conn.Instantiate() as RoomConnection;
				connInstance.TargetRoomId = incoming;
				connInstance.TargetRoomType = graph.GetRoom(incoming).RoomType;
				connInstance.IsEntrance = true;
				// Set the label to show the source room ID and type for debugging purposes
				connInstance.SetLabel(graph.GetRoom(incoming).RoomType.ToString(), isBranch);
				// Add the connection to this room's entrances
				entrances.GetChildOrNull<Marker3D>(entranceIndex++)?.AddChild(connInstance);
				connInstance.connection_enabled = true;
			}

			// Instantiate outgoing connections for the room
			foreach (int outgoing in room.OutgoingConnections)
			{
				bool isBranch = graph.GetRoom(outgoing).IsOnMainPath;
				PackedScene conn = ConnectionScene;
				RoomConnection connInstance = conn.Instantiate() as RoomConnection;
				connInstance.TargetRoomId = outgoing;
				connInstance.TargetRoomType = graph.GetRoom(outgoing).RoomType;
				connInstance.IsEntrance = false;
				// Set the label to show the target room ID and type for debugging purposes
				connInstance.SetLabel(graph.GetRoom(outgoing).RoomType.ToString(), isBranch);
				// Add the connection to this room's exits
				exits.GetChildOrNull<Marker3D>(exitIndex++)?.AddChild(connInstance);

				// Gate combat and event rooms until cleared
				if (room.RoomType == RoomTypes.Combat || room.RoomType == RoomTypes.Event)
				{
					connInstance.connection_enabled = false;
					connInstance.Visible = false;
				}
			}

			// Add the room to the Rooms node in the scene tree
			roomsRoot.AddChild(roomNode);

			// If it is a combat room, spawn enemies randomly in the enemy area node based off difficulty
			if (room.RoomType == RoomTypes.Combat)
			{
				int enemyCount = GameDifficultyManager.Instance.getEnemyCount();
				var enemyNode = roomNode.GetNodeOrNull<Node3D>("Enemies");
				List<Marker3D> spawnPoints = roomNode.GetNodeOrNull<Node3D>("EnemySpawnArea").GetChildren().OfType<Marker3D>().ToList();
				enemyCount = Math.Min(enemyCount, spawnPoints.Count);

				int floor = GameDifficultyManager.Instance.CurrentFloor;
				float floorMult = 1.0f + (floor - 1) * 0.35f;

				for (int i = 0; i < enemyCount; i++)
				{
					EnemyFSM enemyInstance = GD.Load<PackedScene>("res://game/entity/enemy_fsm/enemy_patroller.tscn").Instantiate() as EnemyFSM;
					enemyInstance.Name = $"Enemy_{i}";

					if (enemyInstance.healthComponent != null)
					{
						enemyInstance.healthComponent.MaxHealth *= floorMult;
					}
					if (enemyInstance.attackComponent != null)
					{
						enemyInstance.attackComponent.BaseDamage *= floorMult;
					}

					enemyNode.AddChild(enemyInstance);
					enemyInstance.GlobalPosition = spawnPoints[i].GlobalPosition + new Vector3(0, 0.9F, 0);
				}

				GD.Print($"[Floor {floor}] Spawned {enemyCount} enemies in Room {room.Id} (difficulty {GameDifficultyManager.Instance.getCurrentDifficultyScore():F2}, floor mult {floorMult:F2}x).");
			}
		}
	}

	private Node3D CreateRoomNode(int roomId, RoomConfig config)
	{
		if (config?.Scene == null)
		{
			GD.PushWarning($"RoomConfig for room {roomId} has no Scene assigned.");
			return null;
		}

		Node3D instance = config.Scene.Instantiate<Node3D>();
		instance.Name = $"Room_{roomId}";
		instance.SetMeta("RoomId", roomId);
		instance.SetMeta("RoomType", config.RoomType.ToString());
		return instance;
	}

	private void SpawnPlayer()
	{
		Node3D entranceRoom = GetRoomNode(0);
		if (entranceRoom == null)
		{
			GD.PushError("DungeonGenerator: Could not find entrance room (Room_0).");
			return;
		}

		Marker3D enterPoint = entranceRoom.GetNodeOrNull<Marker3D>("EnterPoint");
		if (enterPoint == null)
		{
			GD.PushError("DungeonGenerator: Entrance room has no 'EnterPoint' Marker3D.");
			return;
		}

		if (_player == null || !IsInstanceValid(_player))
		{
			if (PlayerScene == null)
			{
				GD.PushError("DungeonGenerator: PlayerScene is not assigned.");
				return;
			}
			_player = PlayerScene.Instantiate<Player>();
		}

		entranceRoom.AddChild(_player);
		_player.Transform = enterPoint.Transform;

		// Boss-victory path in BattleManager skips the normal input/mouse reset, so do it here
		_player.Velocity = Vector3.Zero;
		_player.SetAcceptKeyboardInput(true);
		Input.MouseMode = Input.MouseModeEnum.Captured;
	}

	private Node3D GetOrCreateRoot(string name)
	{
		// Get or create the root node with the given name
		Node3D root = GetNodeOrNull<Node3D>(name);
		if (root == null)
		{
			root = new Node3D();
			root.Name = name;
			AddChild(root);
		}
		return root;
	}

	private void ClearChildren(Node root)
	{
		// Clear all children of this root node immediately to prevent issues with stale connections
		foreach (Node child in root.GetChildren())
		{
			root.RemoveChild(child);
			child.QueueFree();
		}
	}
}
