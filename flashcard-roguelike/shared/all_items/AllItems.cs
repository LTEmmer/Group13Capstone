using Godot;
using Godot.Collections;
using System;
using System.Collections.Generic;

public partial class AllItems : Node
{
    public static AllItems Instance { get; private set; }

    public Array<ItemResource> ItemResources { get; private set; } = new();

    private readonly string[] _paths =
    {
        "res://game/entity/item/item_resources/stats",
        "res://game/entity/item/item_resources/use",
        "res://game/entity/item/item_resources/tool"
    };

    public override void _EnterTree()
    {
        // Prevent duplicate instances if scene reloads
        if (Instance != null)
        {
            QueueFree();
            return;
        }

        Instance = this;
    }

    public override void _Ready()
    {
        LoadAllItems();
    }

    private void LoadAllItems()
    {
        ItemResources.Clear();

        foreach (var path in _paths)
        {
            var dir = DirAccess.Open(path);
            if (dir == null)
            {
                GD.PrintErr($"Failed to open directory: {path}");
                continue;
            }

            dir.ListDirBegin();
            string file;

            while ((file = dir.GetNext()) != "")
            {
                if (file == "." || file == "..")
                    continue;

                if (dir.CurrentIsDir())
                    continue;

                if (!file.EndsWith(".tres") && !file.EndsWith(".res"))
                    continue;

                var resourcePath = $"{path}/{file}";
                var resource = ResourceLoader.Load<ItemResource>(resourcePath);

                if (resource != null)
                {
                    ItemResources.Add(resource);
                }
                else
                {
                    GD.PrintErr($"Failed to load ItemResource: {resourcePath}");
                }
            }

            dir.ListDirEnd();
        }

        GD.Print($"Loaded {ItemResources.Count} items.");
    }
}