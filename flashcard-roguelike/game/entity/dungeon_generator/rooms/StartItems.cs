using Godot;

public partial class StartItems : Node3D
{
    [Export] public PackedScene ItemScene;
    [Export] public float Radius = 4f;
    [Export] public int ItemCount = 4;

    public override void _Ready()
    {
        SpawnItems();
    }

	private void SpawnItems()
	{
    	if (AllItemsManager.Instance == null) { GD.PrintErr("[StartItems] AllItems not ready."); return; }
    	if (ItemScene == null)         { GD.PrintErr("[StartItems] ItemScene not assigned."); return; }

    	var items = AllItemsManager.Instance.GetRandomItems(ItemCount, allowDuplicates: false);
    	if (items == null) return;

    	for (int i = 0; i < items.Count; i++)
    	{
        	float angle = i * (2f * Mathf.Pi / items.Count);
        	var offset = new Vector3(Mathf.Cos(angle) * Radius, 0f, Mathf.Sin(angle) * Radius);

        	var itemNode = ItemScene.Instantiate<Item>();
        	AddChild(itemNode);
        	itemNode.Position = offset;
        	itemNode.Init(items[i]);

        	GD.Print($"[StartItems] Spawned '{items[i].Name}' at {itemNode.GlobalPosition}");
    	}
	}
}