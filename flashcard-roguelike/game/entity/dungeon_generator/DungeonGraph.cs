using System;
using System.Collections.Generic;
using Godot;

public enum RoomTypes
{
	Entrance = 0,
	Combat = 1,
	Event = 2,
	Treasure = 3,
	Exit = 4
}

public sealed class DungeonRoom
{
	public int Id { get; }
	public RoomConfig Config { get; }
	public RoomTypes RoomType => Config.RoomType;
	public int MaxConnections => Config.MaxConnections;
	public HashSet<int> IncomingConnections { get; } = new HashSet<int>();
	public HashSet<int> OutgoingConnections { get; } = new HashSet<int>();
	public int Depth { get; set; }
	public float Difficulty { get; set; }

	public DungeonRoom(int id, RoomConfig config, int depth)
	{
		Id = id;
		Config = config;
		Depth = depth;
	}
}

public sealed class DungeonGraph
{
	public List<DungeonRoom> Rooms { get; }

	public DungeonGraph(List<RoomConfig> orderedConfigs)
	{
		if (orderedConfigs == null || orderedConfigs.Count <= 1)
			throw new ArgumentOutOfRangeException(nameof(orderedConfigs), "Must provide at least 2 room configs.");

		Rooms = new List<DungeonRoom>(orderedConfigs.Count);
		for (int i = 0; i < orderedConfigs.Count; i++)
		{
			Rooms.Add(new DungeonRoom(i, orderedConfigs[i], i));
			GD.Print($"Created room {i} of type {orderedConfigs[i].RoomType}");
		}
	}

	public DungeonRoom GetRoom(int id)
	{
		return Rooms[id];
	}

	public bool AreConnected(int from, int to)
	{
		return Rooms[from].OutgoingConnections.Contains(to) || Rooms[to].IncomingConnections.Contains(from);
	}

	public bool TryConnect(int from, int to)
	{
		if (from == to) return false;

		DungeonRoom fromRoom = Rooms[from];
		DungeonRoom toRoom = Rooms[to];

		// Prevent direct connection from entrance to exit
		if (fromRoom.RoomType == RoomTypes.Entrance && toRoom.RoomType == RoomTypes.Exit)
			return false;

		// Ensure exits don't go out, and entrances don't allow in
		if (fromRoom.RoomType == RoomTypes.Exit || toRoom.RoomType == RoomTypes.Entrance)
			return false;

		// Ensure rooms only go "deeper"
		if (toRoom.Depth <= fromRoom.Depth)
			toRoom.Depth = fromRoom.Depth + 1;

		// Each room enforces its own MaxConnections from its config
		if (fromRoom.OutgoingConnections.Count >= fromRoom.MaxConnections ||
			toRoom.IncomingConnections.Count >= toRoom.MaxConnections)
			return false;

		// Ensure the connection doesn't already exist
		if (fromRoom.OutgoingConnections.Contains(to))
		{
			return false;
		}

		fromRoom.OutgoingConnections.Add(to);
		toRoom.IncomingConnections.Add(from);
		return true;
	}

	public void PrintGraph()
	{
		GD.Print("=== DUNGEON GRAPH ===");

		foreach (DungeonRoom room in Rooms)
		{
			string outgoing = room.OutgoingConnections.Count > 0 ? string.Join(", ", room.OutgoingConnections) : "none";
			string incoming = room.IncomingConnections.Count > 0 ? string.Join(", ", room.IncomingConnections): "none";
			GD.Print($"Room {room.Id} [{room.RoomType}] | Out → {outgoing} | In ← {incoming}");
		}

		GD.Print("=====================");
	}

	public bool AreAllRoomsReachable()
	{
		if (Rooms.Count == 0) return true;
	
		HashSet<int> visited = new HashSet<int>();
		Queue<int> toVisit = new Queue<int>();
	
		// Start from the entrance (room 0)
		toVisit.Enqueue(0);
		visited.Add(0);
	
		// BFS traversal following outgoing connections
		while (toVisit.Count > 0)
		{
			int current = toVisit.Dequeue();
			DungeonRoom room = Rooms[current];
	
			foreach (int nextRoomId in room.OutgoingConnections)
			{
				if (!visited.Contains(nextRoomId))
				{
					visited.Add(nextRoomId);
					toVisit.Enqueue(nextRoomId);
				}
			}
		}
	
		return visited.Count == Rooms.Count;
	}
}
