using Godot;

public partial class CurrentRoomManager : Node
{
	public static CurrentRoomManager Instance { get; private set; }

	[Signal] public delegate void RoomChangedEventHandler(int newRoomId);
	[Signal] public delegate void GraphChangedEventHandler();

	private int _currentRoomId = -1;
	public int CurrentRoomId
	{
		get => _currentRoomId;
		set { _currentRoomId = value; EmitSignal(SignalName.RoomChanged, value); }
	}

	private DungeonGraph _graphRef;
	public DungeonGraph GraphRef
	{
		get => _graphRef;
		set { _graphRef = value; EmitSignal(SignalName.GraphChanged); }
	}

	public override void _Ready()
	{
		Instance = this;
	}
}
