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

	public DungeonRoom(int id, RoomTypes type)
	{
		Id = id;
		RoomType = type;
	}
}

public sealed class DungeonGraph
{
	public List<DungeonRoom> Rooms { get; }

	public DungeonGraph(int roomCount)
	{
		RoomTypes type;
		Random rand = new Random();

		if (roomCount <= 1)
		{
			throw new ArgumentOutOfRangeException(nameof(roomCount), "Room count must be greater than 1.");
		}

		Rooms = new List<DungeonRoom>(roomCount);
		Rooms.Add(new DungeonRoom(0, RoomTypes.Entrance)); // Ensure the first room is always an entrance

		for (int i = 1; i < roomCount - 1; i++)
		{
			type = (RoomTypes)rand.Next(1, Enum.GetNames(typeof(RoomTypes)).Length - 1);
			Rooms.Add(new DungeonRoom(i, type));
			GD.Print($"Created room {i} of type {type}");
		}

		Rooms.Add(new DungeonRoom(roomCount - 1, RoomTypes.Exit)); // Ensure the last room is always an exit
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
		if (from == to)
		{
			return false;
		}

		DungeonRoom fromRoom = Rooms[from];
		DungeonRoom toRoom = Rooms[to];

		if (fromRoom.OutgoingConnections.Count >= maxConnections || toRoom.IncomingConnections.Count >= maxConnections)
		{
			return false;
		}

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
}
