using Godot;

public partial class CurrentRoomManager : Node
{
	public static CurrentRoomManager Instance { get; private set; }

	public int CurrentRoomId { get; set; } = -1;

	public override void _Ready()
	{
		Instance = this;
	}
}
