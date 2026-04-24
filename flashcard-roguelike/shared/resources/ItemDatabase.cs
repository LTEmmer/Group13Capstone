using Godot;
using Godot.Collections;

[GlobalClass]
public partial class ItemDatabase : Resource
{
    [Export]
    public Array<ItemResource> Items = new();
}