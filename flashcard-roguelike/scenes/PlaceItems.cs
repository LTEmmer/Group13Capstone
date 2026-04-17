using Godot;
using System;

public partial class PlaceItems : Node3D
{
    [Export] public PackedScene ItemScene;
    [Export] public float Spacing = 2f;

    public override void _Ready()
    {
        SpawnAllItems();
    }

    private void SpawnAllItems()
    {
        int index = 0;

        foreach (var resource in AllItems.Instance.ItemResources)
        {
            SpawnItem(resource, index);
            index++;
        }
    }

    private void SpawnItem(ItemResource resource, int index)
    {
        var item = ItemScene.Instantiate<Item>();
        AddChild(item);

        Vector3 startPosition = GlobalPosition;
        Vector3 offset = new Vector3(index * 1.1f, 0, 0);

        item.GlobalPosition = startPosition + offset;

        item.Init(resource);
    }
}