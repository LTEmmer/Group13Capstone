using Godot;

[GlobalClass]
public partial class LootEntry : Resource
{
    [Export] public ItemResource Item;
    [Export] public Mesh Mesh;
}