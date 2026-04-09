using System;
using Godot;

public partial class Item : Interactable 
{
	[Export] private MeshInstance3D _meshInstance;
	[Export] public ItemResource _resource;

	public override void _Ready()
	{
		if (_resource == null) return;

		if (_meshInstance != null && _resource.Mesh != null)
			_meshInstance.Mesh = _resource.Mesh;
	}
	public override void Interact(Node caller)
	{
		GD.Print("ALKSJDLAKSJDLKa");
		if (_resource == null || caller is not Player player) return;

		AudioManager.Instance.PlayItemPickupSound();

		if(_resource.UseEffects == null && _resource.PickupEffects == null)
		{
			GD.Print(_resource.Name + ": has no effects");
		}

		if (_resource.AddToInventory){
			var inventory = player.GetNodeOrNull<InventoryComponent>("InventoryComponent");
			if (inventory == null) 
				throw new ArgumentNullException("Player has no inventory!");
			inventory.AddItem(_resource, 1);
			GD.Print($"Added {1} '{_resource.Name}' to {player.Name}'s inventory.");
		}

		if (_resource.PickupEffects != null)
		{
			foreach (var effect in _resource.PickupEffects)
				effect.Apply(player, new ItemInstance(_resource));
		}
		

		QueueFree();
	}

}
