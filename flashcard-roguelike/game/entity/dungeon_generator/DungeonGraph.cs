using System;
using System.Collections.Generic;

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
		Rooms.Add(new DungeonRoom(roomCount - 1, RoomTypes.Exit)); // Ensure the last room is always an exit

		for (int i = 1; i < roomCount - 1; i++)
		{
			type = (RoomTypes)rand.Next(1, Enum.GetNames(typeof(RoomTypes)).Length);
			Rooms.Add(new DungeonRoom(i, type));
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

	public bool TryConnect(int from, int to, int maxConnections)
	{
		if (from == to)
		{
			return false;
		}

		DungeonRoom roomA = Rooms[from];
		DungeonRoom roomB = Rooms[to];

		if (roomA.OutgoingConnections.Count >= maxConnections || roomB.IncomingConnections.Count >= maxConnections)
		{
			return false;
		}

		if (roomA.OutgoingConnections.Contains(to))
		{
			return false;
		}

		roomA.OutgoingConnections.Add(to);
		roomB.IncomingConnections.Add(from);
		return true;
	}
}
