using Godot;
using System.Collections.Generic;

public enum SceneNames
{
    MainMenu,
    Dungeon,
    TestArea,
    PauseMenu,
    Player,
}

/// <summary>
/// Singleton for loading, showing, and switching between scenes. Access via <see cref="Instance"/>.
/// Register scenes by adding entries to both the <see cref="SceneNames"/> enum and <see cref="Scenes"/> dictionary.
/// <br/><br/>
/// Scenes are automatically sorted into typed containers under the Main node:
/// <list type="bullet">
///   <item><description><c>Node3D</c> roots → <b>World3D</b> (a Node3D)</description></item>
///   <item><description><c>Node2D</c> roots → <b>World2D</b> (a Node2D)</description></item>
///   <item><description><c>Control</c> roots → <b>UI</b> (a CanvasLayer)</description></item>
/// </list>
/// If a container doesn't exist under Main it is created automatically.
/// <br/><br/>
/// <see cref="Load"/> instantiates a scene without showing it, useful to load in the background before you need it.
/// <see cref="Free"/> destroys it and removes it from memory (it must be hidden first).
/// <br/>
/// <see cref="SetVisibility"/> shows or hides any scene without switching away from the current one. Use for HUDs and overlays.
/// <see cref="ToggleVisability"/> flips it.
/// <br/>
/// <see cref="Navigate"/> switches to a scene, hides the current one and shows the new one.
/// Going back is up to the caller, just call <see cref="Navigate"/> with the desired destination.
/// <br/><br/>
/// <code>
/// SceneManager.Instance.Navigate(SceneNames.MainMenu);
/// SceneManager.Instance.Load(SceneNames.Dungeon);      // load in background
/// SceneManager.Instance.Navigate(SceneNames.Dungeon);  // switch instantly
/// SceneManager.Instance.SetVisibility(SceneNames.PauseMenu, true); // overlay
/// </code>
/// </summary>
public partial class SceneManager : Node
{
    public static SceneManager Instance { get; private set; }

    [Signal] public delegate void SceneFocusedEventHandler(string sceneName);

    private readonly Dictionary<SceneNames, SceneData> Scenes = new()
    {
        { SceneNames.MainMenu,  new SceneData("MainMenu",  "res://game/ui/main_menu/main_menu.tscn") },
        { SceneNames.Dungeon,   new SceneData("Dungeon",   "res://game/entity/dungeon_generator/dungeon_generator.tscn") },
        { SceneNames.TestArea,  new SceneData("TestArea",  "res://scenes/test_room.tscn") },
        { SceneNames.PauseMenu, new SceneData("PauseMenu", "res://game/ui/pause_menue/pause_menu.tscn") },
    };

    [Export] public NodePath MainPath { get; set; } = "/root/Main";

    // Container node names created (or found) under Main.
    private const string Container3D = "World3D";
    private const string Container2D = "World2D";
    private const string ContainerUI = "UI";

    private readonly Dictionary<SceneNames, Node>   _loaded  = new();
    private readonly HashSet<SceneNames>            _visible = new();
    private SceneNames? _focused;

    private Node        _root;
    private Node3D      _world3D;
    private Node2D      _world2D;
    private CanvasLayer _ui;

    public override void _Ready()
    {
        Instance = this;
        CallDeferred(nameof(Init));
    }

    private void Init()
    {
        _root = GetNode(MainPath);
        if (_root == null)
        {
            GD.PrintErr($"[SceneManager] Node not found at '{MainPath}'");
            return;
        }

        _world3D = GetOrCreateChild<Node3D>(_root, Container3D);
        _world2D = GetOrCreateChild<Node2D>(_root, Container2D);
        _ui = GetOrCreateChild<CanvasLayer>(_root, ContainerUI);
    }

    // Returns an existing child by name, or creates and attaches one.
    private static T GetOrCreateChild<T>(Node parent, string childName) where T : Node, new()
    {
        var existing = parent.GetNodeOrNull<T>(childName);
        if (existing != null) return existing;

        var node = new T { Name = childName };
        parent.AddChild(node);
        GD.Print($"[SceneManager] Created container '{childName}' under '{parent.Name}'");
        return node;
    }

    // Picks the right container for an instantiated node.
    private Node ContainerFor(Node node) => node switch
    {
        Control => _ui,
        Node2D => _world2D,
        Node3D => _world3D,
        _ => _root,
    };

    // ------------------------------------------------------------------
    // Public API
    // ------------------------------------------------------------------

    /// <summary>Instantiates a scene invisibly without showing it. No-op if already loaded.</summary>
    public void Load(SceneNames key)
    {
        if (_loaded.ContainsKey(key)) return;

        var packed = GD.Load<PackedScene>(Scenes[key].path);
        if (packed == null)
        {
            GD.PrintErr($"[SceneManager] Could not load '{Scenes[key].path}'");
            return;
        }

        var node      = packed.Instantiate();
        var container = ContainerFor(node);

        node.Set("visible", false);
        container.AddChild(node);
        _loaded[key] = node;

        GD.Print($"[SceneManager] Loaded '{Scenes[key].name}' → {container.Name}");
    }

    /// <summary>Frees a scene. It must not be visible, call SetVisibility(key, false) first.</summary>
    public void Free(SceneNames key)
    {
        if (_visible.Contains(key))
        {
            GD.PrintErr($"[SceneManager] '{key}' is still visible, hide it first.");
            return;
        }

        if (_loaded.TryGetValue(key, out var node))
        {
            node.QueueFree();
            _loaded.Remove(key);
            if (_focused == key) _focused = null;
            GD.Print($"[SceneManager] Freed '{Scenes[key].name}'");
        }
    }

    /// <summary>
    /// Shows or hides a scene. Loads it first if needed.
    /// Does not change focus, use <see cref="Navigate"/> for that.
    /// </summary>
    public void SetVisibility(SceneNames key, bool visible)
    {
        if (visible) Load(key);
        if (!_loaded.TryGetValue(key, out var node)) return;

        node.Set("visible", visible);
        if (visible) _visible.Add(key);
        else         _visible.Remove(key);
    }

    /// <summary>Flips the current visibility of a scene.</summary>
    public void ToggleVisability(SceneNames key) => SetVisibility(key, !_visible.Contains(key));

    /// <summary>
    /// Hides the current focused scene, shows the new one, and records it as focused.
    /// Use <see cref="SetVisibility"/> for overlays or HUDs that sit outside the main focus flow.
    /// </summary>
    public void Navigate(SceneNames key)
    {
        if (_focused == key) return;

        if (_focused.HasValue)
            SetVisibility(_focused.Value, false);

        SetVisibility(key, true);
        _focused = key;

        EmitSignal(SignalName.SceneFocused, Scenes[key].name);
        GD.Print($"[SceneManager] Focused '{Scenes[key].name}'");
    }

    // ------------------------------------------------------------------
    // Queries
    // ------------------------------------------------------------------

    public SceneNames? Focused => _focused;
    public bool IsLoaded(SceneNames key)  => _loaded.ContainsKey(key);
    public bool IsVisible(SceneNames key) => _visible.Contains(key);

    /// <summary>Returns the root node of a loaded scene cast to T, or null.</summary>
    public T Get<T>(SceneNames key) where T : Node =>
        _loaded.TryGetValue(key, out var n) ? n as T : null;
}