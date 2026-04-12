using System;
using Godot;

public partial class Item : Interactable 
{
	[Export] private MeshInstance3D _meshInstance;
	[Export] public ItemResource _resource;
	[Export] private Label3D label;

	public override void _Ready()
	{
    	if (_resource == null) return;
    	if (_meshInstance != null && _resource.Mesh != null)
        	_meshInstance.Mesh = _resource.Mesh;

    	label.Text = _resource?.Name;
    	label.Visible = false;
		label.Billboard = BaseMaterial3D.BillboardModeEnum.Enabled;
	}

	public override void Interact(Node caller)
	{
		if (_resource == null || caller is not Player player) return;

		AudioManager.Instance.PlayItemPickupSound();

		if(_resource.UseEffects == null && _resource.PickupEffects == null)
		{
			GD.Print(_resource.Name + ": has no effects");
		}

		if (_resource.AddToInventory){
			var inventoryNode = player.FindChild("InventoryComponent");
			GD.Print("Found node: " + inventoryNode?.Name ?? "null");
			var inventory = inventoryNode as InventoryComponent;
			if (inventory == null) 
				throw new ArgumentNullException("Player has no inventory!");
			inventory.AddItem(new ItemInstance(_resource));
			GD.Print($"Added '{_resource.Name}' to {player.Name}'s inventory.");
		}

		if (_resource.PickupEffects != null)
		{
			foreach (var effect in _resource.PickupEffects)
				effect.Apply(player, new ItemInstance(_resource));
		}

		TaloTelemetry.TrackItemsPickedUp();
		

		QueueFree();
	}

    public override void HoverStart(Node caller)
	{
		label.Visible = true;
	}
    public override void HoverEnd(Node caller)
	{
		label.Visible = false;
	}
}
