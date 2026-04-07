using Godot;

public partial class InventoryUI : Node3D
{
    [Export] private InventoryComponent _inventory;
    [Export] private ItemList _itemList;

    // Left page
    [Export] private SubViewport _leftPageViewport;
    [Export] private StaticBody3D _leftPageCollider;

    // Right page
    [Export] private SubViewport _rightPageViewport;
    [Export] private StaticBody3D _rightPageCollider;
    [Export] private Button _equipButton;
    [Export] private Button _dropButton;
    [Export] private Button _inspectButton;

    [Export] Texture2D testImg;

    private const float PageHalfW = 4.92f / 2f;
    private const float PageHalfH = 7.17f / 2f;

    private Camera3D _camera;
    private readonly System.Collections.Generic.List<ItemInstance> _items = new();

    public override void _Ready()
    {
        ProcessMode = ProcessModeEnum.Always;
        _camera = GetParent<Camera3D>();

        _inventory.ItemAdded   += OnItemAdded;
        _inventory.ItemRemoved += OnItemRemoved;
        foreach (var item in _inventory.Items)
            OnItemAdded(item);

        _itemList.ItemSelected += OnItemSelected;

        _equipButton.Pressed   += OnEquipPressed;
        _dropButton.Pressed    += OnDropPressed;
        _inspectButton.Pressed += OnInspectPressed;

        Visible = true;
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event is InputEventMouseButton { Pressed: true } btn)
            TryClickPage(btn.Position);
    }

    private void TryClickPage(Vector2 screenPos)
    {
        var spaceState = GetWorld3D().DirectSpaceState;
        var origin = _camera.ProjectRayOrigin(screenPos);
        var query = PhysicsRayQueryParameters3D.Create(
            origin,
            origin + _camera.ProjectRayNormal(screenPos) * 10f
        );

        var result = spaceState.IntersectRay(query);
        if (result.Count == 0) return;

        var collider = result["collider"].AsGodotObject();

        if (collider == _leftPageCollider)
            HandlePageClick(result, _leftPageCollider, _leftPageViewport, mirrorU: false);
		else if (collider == _rightPageCollider)
    		HandlePageClick(result, _rightPageCollider, _rightPageViewport, mirrorU: false);
    }

    private void HandlePageClick(Godot.Collections.Dictionary result, StaticBody3D collider, SubViewport viewport, bool mirrorU)
    {
        var sprite = collider.GetParent<Sprite3D>();
        Vector3 local = sprite.ToLocal(result["position"].AsVector3());

        float u = Mathf.Clamp((local.X + PageHalfW) / (PageHalfW * 2f), 0f, 1f);
        float v = Mathf.Clamp(1f - (local.Y + PageHalfH) / (PageHalfH * 2f), 0f, 1f);

        if (mirrorU) u = 1f - u;

        var vpSize = (Vector2)viewport.Size;
        var vpPos  = new Vector2(u * vpSize.X, v * vpSize.Y);

        GD.Print($"[Inventory] {(mirrorU ? "Right" : "Left")} page UV ({u:F2}, {v:F2}) → pixel {vpPos}");

        PushViewportClick(viewport, vpPos, true);
        PushViewportClick(viewport, vpPos, false);
    }

    private void PushViewportClick(SubViewport viewport, Vector2 pos, bool pressed)
    {
        viewport.PushInput(new InputEventMouseButton
        {
            Position       = pos,
            GlobalPosition = pos,
            ButtonIndex    = MouseButton.Left,
            Pressed        = pressed
        });
    }

    private void OnItemAdded(ItemInstance item)
    {
        _items.Add(item);
        _itemList.AddItem("TEST", testImg);
    }

    private void OnItemRemoved(ItemInstance item)
    {
        int idx = _items.IndexOf(item);
        if (idx < 0) return;
        _items.RemoveAt(idx);
        _itemList.RemoveItem(idx);
    }

    private void OnItemSelected(long index)
    {
        var item = _items[(int)index];
        GD.Print($"[Inventory] Selected: {item} | Index: {index}");
    }

    private void OnEquipPressed()   => GD.Print("[Inventory] Equip pressed");
    private void OnDropPressed()    => GD.Print("[Inventory] Drop pressed");
    private void OnInspectPressed() => GD.Print("[Inventory] Inspect pressed");
}