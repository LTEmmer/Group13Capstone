using System;
using Godot;

public partial class Item : Interactable
{
    [Export] private MeshInstance3D _meshInstance;
    [Export] public ItemResource _resource;
    [Export] public ItemInstance _itemInstance;
    [Export] private Label3D label;

    // ───────────────────────── INIT ─────────────────────────

    /// <summary>Spawns with a specific instance (e.g. dropped from inventory).</summary>
    public void Init(ItemInstance instance)
    {
        if (instance == null) throw new ArgumentNullException(nameof(instance));
        Setup(instance);
    }

    /// <summary>Spawns from a resource alone creates a fresh instance with Count 1.</summary>
    public void Init(ItemResource resource)
    {
        if (resource == null) throw new ArgumentNullException(nameof(resource));
        Setup(new ItemInstance(resource));
    }

    private void Setup(ItemInstance instance)
    {
        _itemInstance = instance;
        _resource     = instance.Resource;

        if (_meshInstance != null && _resource.Mesh != null)
            _meshInstance.Mesh = _resource.Mesh;

        if (label != null)
        {
            label.Text      = _resource.Name;
            label.Visible   = false;
            label.Billboard = BaseMaterial3D.BillboardModeEnum.Enabled;
        }
    }

    public override void _Ready()
    {
        if (_itemInstance == null && _resource != null)
            Setup(new ItemInstance(_resource));
        else if (_itemInstance != null)
            Setup(_itemInstance);
    }

    public override void Interact(Node caller)
    {
        if (_resource == null || caller is not Player player) return;

        AudioManager.Instance.PlayItemPickupSound();

        if (_resource.UseEffects == null && _resource.PickupEffects == null)
            GD.Print(_resource.Name + ": has no effects");

        if (!_itemInstance.PickupEffectsApplied && _resource.PickupEffects != null)
        {
            foreach (var effect in _resource.PickupEffects)
                effect.Apply(player, _itemInstance);
            _itemInstance.PickupEffectsApplied = true;
        }

        if (_resource.AddToInventory)
        {
            var inventoryNode = player.FindChild("InventoryComponent");
            GD.Print("Found node: " + inventoryNode?.Name ?? "null");
            var inventory = inventoryNode as InventoryComponent;
            if (inventory == null)
                throw new ArgumentNullException("Player has no inventory!");

            inventory.AddItem(_itemInstance);
            GD.Print($"Added '{_resource.Name}' to {player.Name}'s inventory.");
        }

        TaloTelemetry.TrackItemsPickedUp();
        QueueFree();
    }

    // ───────────────────────── HOVER ─────────────────────────

    public override void HoverStart(Node caller) => label.Visible = true;
    public override void HoverEnd(Node caller)   => label.Visible = false;
}