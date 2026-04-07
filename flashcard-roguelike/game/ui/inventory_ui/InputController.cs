using Godot;

public partial class InputController : Node
{
	[Export] private Node3D head;
    [Export] private StaticBody3D _collider;
    [Export] private SubViewport _viewport;
    [Export] private bool _mirrorU = false;
    [Export] private float _pageHalfW = 4.92f / 2f;
    [Export] private float _pageHalfH = 7.17f / 2f;

	private Camera3D _camera;
    private SubViewport _lastHitViewport;
    private Vector2 _lastHitPos;

    public override void _Ready()
    {
        ProcessMode = ProcessModeEnum.Always;
		_camera = head?.GetParent<Camera3D>();
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event is not InputEventMouseButton btn) return;

        if (btn.Pressed)
        {
            if (!TryGetViewportPos(btn.Position, out var vpPos)) return;
            _lastHitPos = vpPos;
            PushClick(vpPos, pressed: true);
        }
        else if (_lastHitPos != Vector2.Zero)
        {
            PushClick(_lastHitPos, pressed: false);
            _lastHitPos = Vector2.Zero;
        }
    }

    private bool TryGetViewportPos(Vector2 screenPos, out Vector2 vpPos)
    {
        vpPos = Vector2.Zero;

        var spaceState = _collider.GetWorld3D().DirectSpaceState;
        var origin     = _camera.ProjectRayOrigin(screenPos);
        var query      = PhysicsRayQueryParameters3D.Create(
            origin,
            origin + _camera.ProjectRayNormal(screenPos) * 10f
        );

        var result = spaceState.IntersectRay(query);
        if (result.Count == 0) return false;
        if (result["collider"].AsGodotObject() != _collider) return false;

        var sprite = _collider.GetParent<Sprite3D>();
        Vector3 local = sprite.ToLocal(result["position"].AsVector3());

        float u = Mathf.Clamp((local.X + _pageHalfW) / (_pageHalfW * 2f), 0f, 1f);
        float v = Mathf.Clamp(1f - (local.Y + _pageHalfH) / (_pageHalfH * 2f), 0f, 1f);
        if (_mirrorU) u = 1f - u;

        vpPos = new Vector2(u * _viewport.Size.X, v * _viewport.Size.Y);
        return true;
    }

    private void PushClick(Vector2 pos, bool pressed)
    {
        _viewport.PushInput(new InputEventMouseButton
        {
            Position       = pos,
            GlobalPosition = pos,
            ButtonIndex    = MouseButton.Left,
            Pressed        = pressed // inverted as established
        });
    }
}