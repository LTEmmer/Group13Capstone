using Godot;
using Godot.Collections;
using System;
using System.Collections.Generic;

public partial class AllItemsManager : Node
{
    public static AllItemsManager Instance { get; private set; }

    public Array<ItemResource> ItemResources { get; private set; } = new();

    private readonly Random _rng = new();

    private const string DATABASE_PATH = "res://shared/all_items/all_items.tres";

    public override void _EnterTree()
    {
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
        var db = GD.Load<ItemDatabase>(DATABASE_PATH);

        if (db == null)
        {
            GD.PrintErr($"[AllItems] Failed to load ItemDatabase at: {DATABASE_PATH}");
            return;
        }

        ItemResources = db.Items;

        GD.Print($"Loaded {ItemResources.Count} items from database.");
    }

    public List<ItemResource> GetRandomItems(int count = 1, bool allowDuplicates = false)
    {
        if (ItemResources.Count == 0)
        {
            GD.PrintErr("[AllItems] No items loaded.");
            return new List<ItemResource>();
        }

        if (!allowDuplicates && count > ItemResources.Count)
            count = ItemResources.Count;

        var results = new List<ItemResource>();
        var exclude = allowDuplicates ? null : new HashSet<ItemResource>();

        for (int i = 0; i < count; i++)
        {
            int totalWeight = 0;

            foreach (var item in ItemResources)
            {
                if (exclude != null && exclude.Contains(item)) continue;
                totalWeight += Mathf.Max(1, 6 - item.Rarity);
            }

            if (totalWeight <= 0)
                break;

            int roll = _rng.Next(0, totalWeight);
            int cumulative = 0;

            foreach (var item in ItemResources)
            {
                if (exclude != null && exclude.Contains(item)) continue;

                cumulative += Mathf.Max(1, 6 - item.Rarity);

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
}