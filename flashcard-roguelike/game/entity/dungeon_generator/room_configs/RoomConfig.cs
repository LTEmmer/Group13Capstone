using Godot;

[GlobalClass]
public partial class RoomConfig : Resource
{
    [Export] public RoomTypes RoomType;
    [Export] public int MaxConnections = 4;
    [Export] public PackedScene Scene;
}
