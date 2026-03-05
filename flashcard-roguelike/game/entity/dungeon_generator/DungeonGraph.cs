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
	public HashSet<int> IncomingConnections { get; } = new HashSet<int>();
	public HashSet<int> OutgoingConnections { get; } = new HashSet<int>();
	public RoomTypes RoomType { get; set; }
	public int Depth { get; set; }
	public float Difficulty { get; set; }

	public DungeonRoom(int id, RoomTypes type, int depth)
	{
		Id = id;
		RoomType = type;
		Depth = depth;
	}
}

public sealed class DungeonGraph
{
	public List<DungeonRoom> Rooms { get; }

	public DungeonGraph(int roomCount, int minCombat = 1, int minEvent = 1, int minTreasure = 1)
	{
		// Initialize the dungeon graph with the specified number of rooms and minimum room type requirements
		RoomTypes type;
		Random rand = new Random();

		if (roomCount <= 1)
		{
			throw new ArgumentOutOfRangeException(nameof(roomCount), "Room count must be greater than 1.");
		}

		Rooms = new List<DungeonRoom>(roomCount);
		Rooms.Add(new DungeonRoom(0, RoomTypes.Entrance, 0)); // Ensure the first room is always an entrance

		int availableRooms = roomCount - 2; // Exclude entrance and exit

		// Add all minimum required rooms to the list first
		List<RoomTypes> roomsToAdd = new List<RoomTypes>();
		for (int i = 0; i < minCombat; i++) roomsToAdd.Add(RoomTypes.Combat);
		for (int i = 0; i < minEvent; i++) roomsToAdd.Add(RoomTypes.Event);
		for (int i = 0; i < minTreasure; i++) roomsToAdd.Add(RoomTypes.Treasure);

		// Fill remaining rooms with random types
		for (int i = roomsToAdd.Count; i < availableRooms; i++)
		{
			roomsToAdd.Add((RoomTypes)rand.Next(1, Enum.GetNames(typeof(RoomTypes)).Length - 1));
		}

		// Shuffle the roomsToAdd list to randomize the order of room types
		for (int i = roomsToAdd.Count - 1; i > 0; i--)
		{
			int j = rand.Next(i + 1);
			// In-place swap of elements at indices i and j
			(roomsToAdd[i], roomsToAdd[j]) = (roomsToAdd[j], roomsToAdd[i]);
		}

		// Add rooms to the graph based on the shuffled list
		for (int i = 0; i < roomsToAdd.Count; i++)
		{
			type = roomsToAdd[i];
			Rooms.Add(new DungeonRoom(i + 1, type, i + 1));
			GD.Print($"Created room {i + 1} of type {type}");
		}

		// Ensure the last room is always an exit
		Rooms.Add(new DungeonRoom(roomCount - 1, RoomTypes.Exit, roomCount - 1)); 
	}

	public DungeonRoom GetRoom(int id)
	{
		return Rooms[id];
	}

	public bool AreConnected(int from, int to)
	{
		return Rooms[from].OutgoingConnections.Contains(to) || Rooms[to].IncomingConnections.Contains(from);
	}

	public bool TryConnect(int from, int to, int maxConnections)
	{
		// If the rooms are the same stop
		if (from == to)
		{
			return false;
		}

		DungeonRoom fromRoom = Rooms[from];
		DungeonRoom toRoom = Rooms[to];

		bool fromIsEntrance = fromRoom.RoomType == RoomTypes.Entrance;
		bool toIsExit = toRoom.RoomType == RoomTypes.Exit;

		// Prevent entrance from having too many outgoing connections, 2 for now
		if (fromIsEntrance && fromRoom.OutgoingConnections.Count >= 2)
		{
			return false; 
		}

		// Prevent exit from having having too many incoming connections, 2 for now
		if (toIsExit && toRoom.IncomingConnections.Count >= 2)
		{
			return false; 
		}

		// Prevent direct connection from entrance to exit
		if (fromIsEntrance && toIsExit)
		{
			return false; 
		}

		// Ensure exits don't go out, and entrances don't allow in
		if (fromRoom.RoomType == RoomTypes.Exit || toRoom.RoomType == RoomTypes.Entrance)
		{
			return false;
		}

		// Ensure rooms only go "deeper"
		if (toRoom.Depth <= fromRoom.Depth)
		{
			toRoom.Depth = fromRoom.Depth + 1; // Update depth to maintain proper layering
		}

		// Ensure rooms don't exceed max connections
		if (fromRoom.OutgoingConnections.Count >= maxConnections || toRoom.IncomingConnections.Count >= maxConnections)
		{
			return false;
		}

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