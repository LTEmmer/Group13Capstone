using Godot;
using Godot.Collections;
using System;

[GlobalClass]
public partial class InventoryResource : Resource
{
	[Export] public Array<ItemInstance> inventory;
}
