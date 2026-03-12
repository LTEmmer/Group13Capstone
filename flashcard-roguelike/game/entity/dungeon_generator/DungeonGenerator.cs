using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class DungeonGenerator : Node3D
{
	[Export] public int MaxRoomCount = 8;
	[Export] public int MaxConnections = 4;
	[Export] public int MinCombatRooms = 1;
	[Export] public int MinEventRooms = 1;
	[Export] public int MinTreasureRooms = 1;
	[Export] public bool UseRandomSeed = true;
	[Export] public int Seed = 1234;
	[Export] public PackedScene EntranceRoomScene;
	[Export] public PackedScene ExitRoomScene;
	[Export] public PackedScene[] CombatRoomScenes;
	[Export] public PackedScene[] EventRoomScenes;
	[Export] public PackedScene[] TreasureRoomScenes;
	[Export] public PackedScene ConnectionScene;

	private readonly RandomNumberGenerator _rng = new RandomNumberGenerator();

    // Move stuff from Ready to a separate GenerateDungeon method that can be called 
    // whenever we want to generate a new dungeon, such as when the player enters a new floor or restarts after death.
    // Everytime we generate the graph, each room should be assigned a difficulty rating from the 
    // difficulty singleton. This determins the amount of enemies in combat rooms, the quality of loot in treasure rooms, 
    // and the difficulty of flashcard challenges in event rooms. Difficulty can be assigned in the 
    // spawn rooms method, and can be stored in the DungeonRoom class as a property. 
    // This way, the difficulty of each room can be easily accessed when spawning the room and its contents, 
    // and can also be used by the UI to display the difficulty of each room to the player.
    public override void _Ready()
    {
        DungeonGraph graph = GenerateGraph(); // Generate the dungeon graph structure
		graph.PrintGraph();
		Dictionary<int, Vector3> positions = GenerateLayout(graph); // Generate world positions for rooms
		SpawnRooms(graph, positions); // Instantiate room scenes and connections based on the graph and layout

		if (CurrentRoomManager.Instance != null)
			CurrentRoomManager.Instance.CurrentRoomId = 0;
	}


	public DungeonGraph GenerateGraph()
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

		// Ensure that MaxRoomCount is sufficient to accommodate the minimum required rooms
		int minRoomCount = MinCombatRooms + MinEventRooms + MinTreasureRooms + 2; // +2 for entrance and exit
		if (MaxRoomCount < minRoomCount)
		{
			GD.PushError($"MaxRoomCount must be at least {minRoomCount} to accommodate the minimum required rooms. Adjusting MaxRoomCount to {minRoomCount}.");
			MaxRoomCount = minRoomCount;
		}
		int roomCount = _rng.RandiRange(minRoomCount, MaxRoomCount);

		// Create the dungeon graph with the specified number of rooms and minimum room type requirements
		DungeonGraph graph = new DungeonGraph(roomCount, MinCombatRooms, MinEventRooms, MinTreasureRooms);
		HashSet<int> connected = new HashSet<int> { 0 }; // Start with entrance
		List<int> toConnect = new List<int>();

		// Initially, all rooms except the entrance are in the toConnect list
		for (int i = 1; i < roomCount; i++)
		{
			toConnect.Add(i);
		}

		// Connect every room to at least one other room, ensuring connectivity from the entrance
		while (toConnect.Count > 0)
		{
			// Randomly select a from room from the connected set and a to room from the toConnect list
			int from = connected.Count <= 0 ? 0 : connected.ElementAt(_rng.RandiRange(0, connected.Count - 1));
			int destIndex = _rng.RandiRange(0, toConnect.Count - 1);
			int to = toConnect[destIndex];

			// Try to connect the rooms, and if successful move the room from toConnect to connected
			if (graph.TryConnect(from, to, MaxConnections))
			{
				connected.Add(to);
				toConnect.RemoveAt(destIndex);
			}
		}

		/* Keep commented out for now to more easily see the graph
		// Add some extra random connections for more interconnectivity and non-linearity
		int targetExtraEdges = roomCount / 3; 
		int attempts = roomCount * roomCount / 2; 
		int added = 0;

		// Randomly connect rooms until we reach the target number of extra edges or exhaust attempts to prevent infinite loops
		while (added < targetExtraEdges && attempts-- > 0)
		{
			int from = _rng.RandiRange(0, roomCount - 1);
			int to = _rng.RandiRange(0, roomCount - 1);

			if (graph.TryConnect(from, to, MaxConnections))
			{
				added++;
			}
		}
		*/

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
			Node3D roomNode = CreateRoomNode(room.Id, room.RoomType);
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
					EnemyExample enemyInstance = GD.Load<PackedScene>("res://game/entity/enemy_example/enemy_example.tscn").Instantiate() as EnemyExample;
					enemyInstance.Name = $"Enemy_{i}";
					enemyNode.AddChild(enemyInstance);
					enemyInstance.GlobalPosition = spawnPoints[i].GlobalPosition;
				}

				GD.Print($"Spawned {enemyCount} enemies in Room {room.Id} based on difficulty score of {GameDifficultyManager.Instance.getCurrentDifficultyScore()}.");
			}	
		}
	}

	private Node3D CreateRoomNode(int roomId, RoomTypes type)
	{	
		// Instantiate rooms based off the room type using switch statement
		Node3D instance = null;
		Node3D roomNode;

		switch (type)
		{
			case RoomTypes.Entrance:
				if (EntranceRoomScene != null)
				{
					instance = EntranceRoomScene.Instantiate() as Node3D;
				}
				break;

			case RoomTypes.Exit:
				if (ExitRoomScene != null)
				{
					instance = ExitRoomScene.Instantiate() as Node3D;
				}
				break;

			case RoomTypes.Combat:
				if (CombatRoomScenes != null && CombatRoomScenes.Length > 0)
				{
					PackedScene scene = CombatRoomScenes[_rng.RandiRange(0, CombatRoomScenes.Length - 1)];
					instance = scene.Instantiate() as Node3D;
				}
				break;

			case RoomTypes.Event:
				if (EventRoomScenes != null && EventRoomScenes.Length > 0)
				{
					PackedScene scene = EventRoomScenes[_rng.RandiRange(0, EventRoomScenes.Length - 1)];
					instance = scene.Instantiate() as Node3D;
				}
				break;

			case RoomTypes.Treasure:
				if (TreasureRoomScenes != null && TreasureRoomScenes.Length > 0)
				{
					PackedScene scene = TreasureRoomScenes[_rng.RandiRange(0, TreasureRoomScenes.Length - 1)];
					instance = scene.Instantiate() as Node3D;
				}
				break;
		}	

		// Assign the instance and metadata, if null print a warning and free the instance to prevent memory leaks
		roomNode = instance;

		if (roomNode != null)
		{
			roomNode.Name = $"Room_{roomId}";
			roomNode.SetMeta("RoomId", roomId);
			roomNode.SetMeta("RoomType", type.ToString());
		}
		else
		{
			instance.QueueFree();
			GD.PushWarning($"No scene found for room type {type}. Room {roomId} will not be instantiated.");
		}

		return roomNode;
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
