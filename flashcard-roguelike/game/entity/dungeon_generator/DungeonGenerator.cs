using Godot;
using System;
using System.Collections.Generic;

public partial class DungeonGenerator : Node3D
{
	[Export] public int MaxRoomCount = 8;
	[Export] public int MaxConnections = 4;
	[Export] public int MaxExtraConnections = 1;
	[Export] public bool UseRandomSeed = true;
	[Export] public int Seed = 1234;
	[Export] public PackedScene EntranceRoomScene;
	[Export] public PackedScene ExitRoomScene;
	[Export] public PackedScene[] CombatRoomScenes;
	[Export] public PackedScene[] EventRoomScenes;
	[Export] public PackedScene[] TreasureRoomScenes;
	[Export] public PackedScene ConnectionScene;

	private readonly RandomNumberGenerator _rng = new RandomNumberGenerator();

	public override void _Ready()
	{
		DungeonGraph graph = GenerateGraph();
		graph.PrintGraph();
		Dictionary<int, Vector3> positions = GenerateLayout(graph);
		SpawnRooms(graph, positions);
	}

	public DungeonGraph GenerateGraph()
	{
		if (UseRandomSeed)
		{
			_rng.Randomize();
		}
		else
		{
			_rng.Seed = (ulong)Seed;
		}

		int roomCount = _rng.RandiRange(4, MaxRoomCount);
		DungeonGraph graph = new DungeonGraph(roomCount);

		List<int> connected = new List<int>();
		List<int> toConnect = new List<int>();

		for (int i = 0; i < roomCount; i++)
		{
			toConnect.Add(i);
		}

		while (toConnect.Count > 0)
		{
			int a = connected.Count <= 0 ? 0 : connected[_rng.RandiRange(0, connected.Count - 1)];
			int destIndex = _rng.RandiRange(0, toConnect.Count - 1);
			int b = toConnect[destIndex];

			if (graph.TryConnect(a, b, MaxConnections))
			{
				connected.Add(b);
				toConnect.RemoveAt(destIndex);
			}
		}

		int targetExtraEdges = _rng.RandiRange(0, MaxExtraConnections);
		int attempts = roomCount * roomCount / 2;
		int added = 0;

		while (added < targetExtraEdges && attempts-- > 0)
		{
			int a = _rng.RandiRange(0, roomCount - 1);
			int b = _rng.RandiRange(0, roomCount - 1);

			if (graph.TryConnect(a, b, MaxConnections))
			{
				added++;
			}
		}

		return graph;
	}

	public Node3D GetRoomNode(int roomId)
	{
		return GetNodeOrNull<Node3D>($"Rooms/Room_{roomId}");
	}

	private Dictionary<int, Vector3> GenerateLayout(DungeonGraph graph)
	{
		int roomCount = graph.Rooms.Count;
		Dictionary<int, Vector3> positions = new Dictionary<int, Vector3>(roomCount);

		for (int i = 0; i < roomCount; i++)
		{
			positions[i] = new Vector3(i * 100f, 0, 0);
		}

		return positions;
	}

	private void SpawnRooms(DungeonGraph graph, Dictionary<int, Vector3> positions)
	{
		Node3D roomsRoot = GetOrCreateRoot("Rooms");
		ClearChildren(roomsRoot);

		foreach (DungeonRoom room in graph.Rooms)
		{	
			Node3D roomNode = CreateRoomNode(room.Id, room.RoomType);
			roomNode.Position = positions[room.Id];
			
			Node3D entrances = roomNode.GetNodeOrNull<Node3D>("Entrances");
			Node3D exits = roomNode.GetNodeOrNull<Node3D>("Exits");

			int entranceIndex = 0;
			int exitIndex = 0;

			if (entrances == null || exits == null)
			{
				GD.PushWarning($"Room {room.Id} is missing an 'Exits' or 'Entrances' Node3D child. Connections will not be properly aligned.");
				continue;
			}

			foreach (int origin in room.IncomingConnections)
			{
				PackedScene conn = ConnectionScene;
				RoomConnection connInstance = conn.Instantiate() as RoomConnection;
				connInstance.TargetRoomId = origin;
				connInstance.IsEntrance = true;
				connInstance.SetLabel(true, origin, graph.GetRoom(origin).RoomType.ToString());
				entrances.GetChildOrNull<Marker3D>(entranceIndex++)?.AddChild(connInstance);
			}

			foreach (int destination in room.OutgoingConnections)
			{
				PackedScene conn = ConnectionScene;
				RoomConnection connInstance = conn.Instantiate() as RoomConnection;
				connInstance.TargetRoomId = destination;
				connInstance.IsEntrance = false;
				connInstance.SetLabel(false, destination, graph.GetRoom(destination).RoomType.ToString());
				exits.GetChildOrNull<Marker3D>(exitIndex++)?.AddChild(connInstance);
			}

			Label3D label = new Label3D();
			label.Text = $"Room {room.Id} ({room.RoomType})";
			label.Position = new Vector3(0, 3, 0);
			label.Billboard = BaseMaterial3D.BillboardModeEnum.Enabled;
			label.FontSize = 94;
			roomNode.AddChild(label);
			
			roomsRoot.AddChild(roomNode);		
		}
	}

	private Node3D CreateRoomNode(int roomId, RoomTypes type)
	{	
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

		roomNode = instance;

		if (roomNode != null)
		{
			roomNode.Name = $"Room_{roomId}";
			roomNode.SetMeta("RoomId", roomId);
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
		foreach (Node child in root.GetChildren())
		{
			child.QueueFree();
		}
	}
}
