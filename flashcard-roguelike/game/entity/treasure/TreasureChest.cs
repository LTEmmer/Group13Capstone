using Godot;

public partial class TreasureChest : Interactable
{
    [Export] public PackedScene ItemScene;
    [Export] public int MinItems = 1;
    [Export] public int MaxItems = 3;
    [Export] public float SpawnRadius = 1.5f;
    [Export] public float SpawnHeight = 0.5f;
    [Export] public float SpawnArcStart = 90f;
    [Export] public float SpawnArcEnd = 260f;
    [Export] public bool IsOpen { get; private set; } = false;

    [Signal] public delegate void ChestOpenedEventHandler(TreasureChest chest);

    [Export] private MeshInstance3D _lidMesh;
    [Export] private Label3D _label;

    private Node3D _sacrificeNode;
    private bool _resolved = false;

    public override void _Ready()
    {
        base._Ready();
        UpdateLabel();
        if (_label != null)
            _label.Visible = false;
    }

    public override void Interact(Node caller) => OpenChest();
    public override void HoverStart(Node caller) { if (_label != null) _label.Visible = true; }
    public override void HoverEnd(Node caller)   { if (_label != null) _label.Visible = false; }

    public void OpenChest()
    {
        if (IsOpen) return;

        IsOpen = true;
        TaloTelemetry.TrackChestsOpened();

        if (_lidMesh != null)
        {
            var tween = CreateTween();
            tween.TweenProperty(_lidMesh, "rotation_degrees:x", -110f, 0.5f)
                .SetTrans(Tween.TransitionType.Bounce)
                .SetEase(Tween.EaseType.Out);
        }

        CreateSacrificeNode();
        SpawnItems();

        UpdateLabel();
        EmitSignal(SignalName.ChestOpened, this);
    }

    private void CreateSacrificeNode()
    {
        _sacrificeNode = new Node3D { Name = "SacrificeNode" };
        GetParent().AddChild(_sacrificeNode);
        _sacrificeNode.GlobalPosition = GlobalPosition;
        _sacrificeNode.ChildExitingTree += OnItemExiting;
    }

    private void OnItemExiting(Node node)
    {
        if (_resolved || node is not Item) return;

        _resolved = true;
        GD.Print("Chest resolved: one item selected");

        if (_sacrificeNode != null && IsInstanceValid(_sacrificeNode))
        {
            _sacrificeNode.QueueFree();
            _sacrificeNode = null;
        }
    }

    private void SpawnItems()
    {
        if (ItemScene == null)
        {
            GD.PushWarning("TreasureChest: Missing ItemScene");
            return;
        }

        int itemCount = (int)GD.RandRange(MinItems, MaxItems);
        var loot = AllItems.Instance.GetRandomItems(itemCount, allowDuplicates: false);
        if (loot == null) return;

        for (int i = 0; i < loot.Count; i++)
        {
            var itemNode = ItemScene.Instantiate<Item>();
            if (itemNode == null) continue;

            itemNode._resource = loot[i];
            _sacrificeNode.AddChild(itemNode);

            float t = loot.Count > 1 ? i / (float)(loot.Count - 1) : 0.5f;
            float angle = Mathf.DegToRad(Mathf.Lerp(SpawnArcStart, SpawnArcEnd, t));

            itemNode.GlobalPosition = GlobalPosition + new Vector3(
                Mathf.Sin(angle) * SpawnRadius,
                SpawnHeight,
                -Mathf.Cos(angle) * SpawnRadius
            );
        }
    }

    private void UpdateLabel()
    {
        if (_label == null) return;
        _label.Text = IsOpen ? "Treasure Chest (Empty)" : "Treasure Chest";
    }
}