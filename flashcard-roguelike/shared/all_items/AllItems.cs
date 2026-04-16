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
    
    private readonly Random _rng = new();

    /// <summary>
    /// Returns a list of random items weighted by rarity.
    /// allowDuplicates = false (default) guarantees all returned items are unique.
    /// </summary>
    public List<ItemResource> GetRandomItems(int count = 1, bool allowDuplicates = false)
    {
        if (ItemResources.Count == 0)
        {
            GD.PrintErr("[AllItems] GetRandomItems called but no items are loaded.");
            return null;
        }

        if (!allowDuplicates && count > ItemResources.Count)
        {
            GD.PrintErr($"[AllItems] Requested {count} unique items but only {ItemResources.Count} exist. Clamping.");
            count = ItemResources.Count;
        }

        var results = new List<ItemResource>();
        var exclude = allowDuplicates ? null : new HashSet<ItemResource>();

        for (int i = 0; i < count; i++)
        {
            int totalWeight = 0;
            foreach (var item in ItemResources)
            {
                if (exclude != null && exclude.Contains(item)) continue;
                totalWeight += 6 - item.Rarity;
            }

            if (totalWeight == 0) break;

            int roll = _rng.Next(0, totalWeight);
            int cumulative = 0;

            foreach (var item in ItemResources)
            {
                if (exclude != null && exclude.Contains(item)) continue;
                cumulative += 6 - item.Rarity;
                if (roll < cumulative)
                {
                    results.Add(item);
                    exclude?.Add(item);
                    break;
                }
            }
        }

        return results;
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