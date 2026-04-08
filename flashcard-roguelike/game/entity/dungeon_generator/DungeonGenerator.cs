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
	
	public override void _Ready()
	{
		AudioManager.Instance?.PlayDungeonMusic();
		// Initialize random number generator with seed
		if (UseRandomSeed)
		{
			_rng.Randomize();
		}
		else
		{
			_rng.Seed = (ulong)Seed;
		}

		DungeonGraph graph = GenerateGraph(); // Generate the dungeon graph structure
		graph.PrintGraph();
		Dictionary<int, Vector3> positions = GenerateLayout(graph); // Generate world positions for rooms
		SpawnRooms(graph, positions); // Instantiate room scenes and connections based on the graph and layout
		SpawnPlayer(); // Place the player at the entrance room's EnterPoint

		if (CurrentRoomManager.Instance != null)
			CurrentRoomManager.Instance.CurrentRoomId = 0;

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

		// Build ordered config list: entrance first, middle shuffled, exit last
		List<RoomConfig> configs = new List<RoomConfig>();
		configs.Add(EntranceConfig); // Added first to ensure its at the start

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
			
		// Create a pool for extra rooms to use
		RoomConfig[][] pools = { CombatConfigs, EventConfigs, TreasureConfigs };
		int remaining = roomCount - 2 - (MinCombatRooms + MinEventRooms + MinTreasureRooms);

		for (int i = 0; i < remaining; i++)
		{
			RoomConfig[] pool = pools[_rng.RandiRange(0, pools.Length - 1)];
			RoomConfig config = pool[_rng.RandiRange(0, pool.Length - 1)];
			GD.Print($"Adding extra room of type {config.RoomType} to meet MaxRoomCount.");
			configs.Add(config);
		}

		// Shuffle middle room configs (index 1 to end, entrance stays at 0)
		for (int i = configs.Count - 1; i > 1; i--)
		{
			int j = _rng.RandiRange(1, i);
			(configs[i], configs[j]) = (configs[j], configs[i]);
		}

		configs.Add(ExitConfig); // Added last to ensure it's at the end
		DungeonGraph graph = new DungeonGraph(configs);

		// Build a shuffled chain: entrance -> [random middle rooms] -> exit.
		// Connecting in order guarantees every room is reachable with no retries or deadlocks.
		int exitId = configs.Count - 1;
		List<int> order = [0]; // entrance first
		for (int i = 1; i < exitId; i++)
			order.Add(i);
		order.Add(exitId); // exit last

		// Shuffle only the middle rooms (keep entrance at [0] and exit at [end])
		for (int i = order.Count - 2; i > 1; i--)
		{
			int j = _rng.RandiRange(1, i);
			(order[i], order[j]) = (order[j], order[i]);
		}

		// Connect each room to the next in the chain
		for (int i = 0; i < order.Count - 1; i++)
			graph.TryConnect(order[i], order[i + 1]);

		// Add extra random connections for branching. TryConnect silently rejects invalid ones
		// (duplicate, over MaxConnections, type violations), so no deadlock risk here.
		int targetExtraEdges = configs.Count / 3;
		GD.Print("Adding up to " + targetExtraEdges + " extra random connections for branching.");

		// We allow more attempts than target edges because many attempts will be rejected, 
		// especially as rooms fill up their connection limits.
		int attempts = configs.Count * configs.Count / 2;

		int added = 0;
		while (added < targetExtraEdges && attempts-- > 0)
		{
			int from = _rng.RandiRange(1, exitId - 1);
			int to = _rng.RandiRange(1, exitId - 1);
			if (graph.TryConnect(from, to))
				added++;
		}

		// Check if all rooms are reachable from the entrance and print a warning if not.
		if (!graph.AreAllRoomsReachable())
		{
			GD.PushWarning("Not all rooms are reachable in the generated dungeon graph.");
		}
		else
		{
			GD.Print("All rooms are reachable in the generated dungeon graph.");
		}

		return graph;
	}

	public Node3D GetRoomNode(int roomId)
	{
		return GetNodeOrNull<Node3D>($"Rooms/Room_{roomId}");
	}

	public string GetRoomDisplayName(int roomId)
	{
		Node3D roomNode = GetRoomNode(roomId);
		if (roomNode == null)
			return "Unknown";
		if (!roomNode.HasMeta("RoomId") || !roomNode.HasMeta("RoomType"))
			return $"Room {roomId}";
		int id = (int)roomNode.GetMeta("RoomId");
		string type = (string)roomNode.GetMeta("RoomType");
		return $"Room {id} ({type})";
	}

	private Dictionary<int, Vector3> GenerateLayout(DungeonGraph graph)
	{
		// We place rooms in a line since they are not physically connected and we want to avoid overlapping.
		int roomCount = graph.Rooms.Count;
		Dictionary<int, Vector3> positions = new Dictionary<int, Vector3>(roomCount);

		// Place each at a fixed distance apart on the x-axis.
		for (int i = 0; i < roomCount; i++)
		{
			positions[i] = new Vector3(i * 100f, 0, 0);
		}

		return positions;
	}

	private void SpawnRooms(DungeonGraph graph, Dictionary<int, Vector3> positions)
	{
		// Get Rooms node and clear exisiting rooms for regeneration.
		Node3D roomsRoot = GetOrCreateRoot("Rooms");
		ClearChildren(roomsRoot);

		// Instantiate each room and its connections based on the graph structure and the generated layout.
		foreach (DungeonRoom room in graph.Rooms)
		{	
			// Create a new room node and set its postition
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
				PackedScene conn = ConnectionScene;
				RoomConnection connInstance = conn.Instantiate() as RoomConnection;
				connInstance.TargetRoomId = incoming;
				connInstance.IsEntrance = true;
				// Set the label to show the source room ID and type for debugging purposes
				connInstance.SetLabel(true, incoming, graph.GetRoom(incoming).RoomType.ToString());
				// Add the connection to this room's entrances
				entrances.GetChildOrNull<Marker3D>(entranceIndex++)?.AddChild(connInstance);
				connInstance.connection_enabled = true;
			}

			// Instantiate outgoing connections for the room
			foreach (int outgoing in room.OutgoingConnections)
			{
				PackedScene conn = ConnectionScene;
				RoomConnection connInstance = conn.Instantiate() as RoomConnection;
				connInstance.TargetRoomId = outgoing;
				connInstance.IsEntrance = false;
				// Set the label to show the target room ID and type for debugging purposes
				connInstance.SetLabel(false, outgoing, graph.GetRoom(outgoing).RoomType.ToString());
				// Add the connection to this room's exits
				exits.GetChildOrNull<Marker3D>(exitIndex++)?.AddChild(connInstance);

				// Gate combat and event rooms until cleared
				if(room.RoomType == RoomTypes.Combat || room.RoomType == RoomTypes.Event) 
				{ 
					connInstance.connection_enabled = false;
					connInstance.Visible = false; //Setting to true for testing purposes
				}
			}

			// Add a label in the middle of the room to show its ID and type for debugging purposes
			Label3D label = new Label3D();
			label.Text = $"Room {room.Id} ({room.RoomType})";
			label.Position = new Vector3(0, 3, 0);
			label.Billboard = BaseMaterial3D.BillboardModeEnum.Enabled;
			label.FontSize = 94;
			roomNode.AddChild(label);
			
			// Add the room to the Rooms node in the scene tree
			roomsRoot.AddChild(roomNode);	

			// If it is a combat room, spawn enemies randomly in the enemy area node based off difficulty
			if (room.RoomType == RoomTypes.Combat)
			{
				int enemyCount = GameDifficultyManager.Instance.getEnemyCount();
				var enemyNode = roomNode.GetNodeOrNull<Node3D>("Enemies");
				List<Marker3D> spawnPoints = roomNode.GetNodeOrNull<Node3D>("EnemySpawnArea").GetChildren().OfType<Marker3D>().ToList();

				for (int i = 0; i < enemyCount; i++)
				{
					// For testing purposes we will just spawn generic enemy instances at random positions in the enemy area
					// In the future, we can use the difficulty score to determine the type and strength of enemies to spawn
					// Maybe we define a list of enemies somewhere with associated difficulty ratings
					//EnemyExample enemyInstance = GD.Load<PackedScene>("res://game/entity/enemy_example/enemy_example.tscn").Instantiate() as EnemyExample;
					EnemyFSM enemyInstance = GD.Load<PackedScene>("res://game/entity/enemy_fsm/enemy_patroller.tscn").Instantiate() as EnemyFSM;
					enemyInstance.Name = $"Enemy_{i}";
					enemyNode.AddChild(enemyInstance);
					enemyInstance.GlobalPosition = spawnPoints[i].GlobalPosition + new Vector3(0,0.9F,0); // Spawner was spawning enemy at waist height
				}

				GD.Print($"Spawned {enemyCount} enemies in Room {room.Id} based on difficulty score of {GameDifficultyManager.Instance.getCurrentDifficultyScore()}.");
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
		if (PlayerScene == null)
		{
			GD.PushError("DungeonGenerator: PlayerScene is not assigned.");
			return;
		}

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

		_player = PlayerScene.Instantiate<Player>();
		_player.Transform = enterPoint.Transform;
		entranceRoom.AddChild(_player);
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
