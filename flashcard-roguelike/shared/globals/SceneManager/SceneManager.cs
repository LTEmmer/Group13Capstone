using Godot;
using System.Collections.Generic;

// ── Scene registry ────────────────────────────────────────────────────────────
// Add a value here and a matching entry in SceneManager.Scenes for every scene
// in your project.
public enum SceneNames
{
    MainMenu,
    Dungeon,
    TestArea,
    Player,
    HUD,
    Inventory,
    GameOver,
    PauseMenu_ButtonPanel,
    PauseMenu_ViewFlashcards,
}

// ── Lifecycle interface ───────────────────────────────────────────────────────
// Implement this on any scene root to receive add/remove callbacks.
// Both methods are optional, only implement what you need.
public interface ISceneLifecycle
{
    void OnSceneAdded()   { }   // called after the node enters the tree
    void OnSceneRemoved() { }   // called just before the node is freed or hidden
}

/// <summary>
/// Singleton scene manager. Access via <see cref="Instance"/>.
/// <br/><br/>
/// Register every scene in the <see cref="SceneNames"/> enum and the
/// <see cref="Scenes"/> dictionary below.
/// <br/><br/>
/// <b>World scenes</b> (3D / 2D), only one active per slot at a time:
/// <code>
/// ChangeScene(SceneNames key)
/// FreeScene(SceneNames key)
/// FreeAll()
/// </code>
/// <b>UI scenes</b>, one current at a time, others kept hidden in memory:
/// <code>
/// SetUI(SceneNames.MainMenu);       // Sets the current UI
/// HideUI();                         // hide current, current = null
/// FreeUI(SceneNames.MainMenu);      // free specific instance
/// ClearUI();                        // free all instances
/// </code>
/// <b>Preloading</b> Loads a scene as hidden:
/// <code>
/// PreloadScene(SceneNames.Dungeon);
/// PreloadUI(SceneNames.MainMenu);
/// </code>
/// <b>Getting instances:</b>
/// <code>
/// DungeonGenerator gen = SceneManager.Instance.Get&lt;DungeonGenerator&gt;(SceneNames.Dungeon);
/// </code>
/// </summary>
public partial class SceneManager : Node
{
    // ── Singleton ─────────────────────────────────────────────────────────────

    public static SceneManager Instance { get; private set; }

    // ── Scene registry ────────────────────────────────────────────────────────

    private readonly Dictionary<SceneNames, SceneData> Scenes = new()
    {
        { SceneNames.MainMenu,                  new SceneData("res://game/ui/main_menu/main_menu.tscn") },
        { SceneNames.Dungeon,                   new SceneData("res://game/entity/dungeon_generator/dungeon_generator.tscn") },
        { SceneNames.TestArea,                  new SceneData("res://scenes/test_room.tscn") },
        { SceneNames.Player,                    new SceneData("res://game/entity/player/player.tscn") },
        { SceneNames.HUD,                       new SceneData("res://game/ui/hud/hud.tscn") },
        { SceneNames.Inventory,                 new SceneData("res://game/ui/inventory_ui/inventory_ui.tscn") },
        { SceneNames.GameOver,                  new SceneData("res://game/ui/game_over/game_over_menu.tscn") },
        { SceneNames.PauseMenu_ButtonPanel,     new SceneData("res://game/ui/pause_menu/button_panel/button_panel.tscn") },
        { SceneNames.PauseMenu_ViewFlashcards,  new SceneData("res://game/ui/pause_menu/view_flashcards/view_flashcards.tscn") },
    };

    // ── Signals ───────────────────────────────────────────────────────────────

    [Signal] public delegate void WorldSceneChangedEventHandler(string key);
    [Signal] public delegate void ScenePreloadedEventHandler(string key);
    [Signal] public delegate void UIChangedEventHandler(string key);
    [Signal] public delegate void UIHiddenEventHandler(string key);

    // ── Configuration ─────────────────────────────────────────────────────────

    [Export] public NodePath MainPath { get; set; } = "/root/Main";

    private const string Container3D = "World3D";
    private const string Container2D = "World2D";
    private const string ContainerUI = "UI";

    // ── Container nodes ───────────────────────────────────────────────────────

    private Node3D      _world3D;
    private Node2D      _world2D;
    private CanvasLayer _ui;

    // ── World scene state ─────────────────────────────────────────────────────

    private Node        _current3D;
    private SceneNames? _current3DKey;
    private Node        _current2D;
    private SceneNames? _current2DKey;

    // ── UI state ──────────────────────────────────────────────────────────────

    // The currently visible UI scene.
    private SceneNames? _currentUI;
    // All instantiated UI nodes, visible or hidden.
    private readonly Dictionary<SceneNames, Node> _uiInstances = new();

    // ── Preload caches ────────────────────────────────────────────────────────

    private readonly Dictionary<SceneNames, string>      _threadedLoads   = new();
    private readonly Dictionary<SceneNames, PackedScene> _preloadedScenes = new();
    private readonly Dictionary<SceneNames, PackedScene> _preloadedUI     = new();

    // ── Lifecycle ─────────────────────────────────────────────────────────────

    public override void _Ready()
    {
        Instance = this;
        CallDeferred(nameof(Init));
    }

    private void Init()
    {
        var root = GetNode(MainPath);
        if (root == null)
        {
            GD.PrintErr($"[SceneManager] Node not found at '{MainPath}'");
            return;
        }

        _world3D = GetOrCreateChild<Node3D>(root, Container3D);
        _world2D = GetOrCreateChild<Node2D>(root, Container2D);
        _ui      = GetOrCreateChild<CanvasLayer>(root, ContainerUI);
    }

    // Poll threaded world-scene preloads each frame.
    public override void _Process(double delta)
    {
        if (_threadedLoads.Count == 0) return;

        var completed = new List<SceneNames>();

        foreach (var (key, path) in _threadedLoads)
        {
            var status = ResourceLoader.LoadThreadedGetStatus(path);

            if (status == ResourceLoader.ThreadLoadStatus.Loaded)
            {
                _preloadedScenes[key] = (PackedScene)ResourceLoader.LoadThreadedGet(path);
                completed.Add(key);
                EmitSignal(SignalName.ScenePreloaded, key.ToString());
                GD.Print($"[SceneManager] Preload complete: '{key}'");
            }
            else if (status == ResourceLoader.ThreadLoadStatus.Failed)
            {
                GD.PrintErr($"[SceneManager] Threaded preload failed: '{key}' at '{path}'");
                completed.Add(key);
            }
        }

        foreach (var key in completed)
            _threadedLoads.Remove(key);
    }

    // ── Public API, World ────────────────────────────────────────────────────

    /// <summary>
    /// Switches to a world scene (3D or 2D), detected from the scene's root node type.
    /// Frees the current scene in that slot. Does not affect UI.
    /// Uses a preloaded resource if available, otherwise loads synchronously.
    /// </summary>
    public void ChangeScene(SceneNames key)
    {
        var instance = InstanceFromKey<Node>(key);
        if (instance == null) return;

        switch (instance)
        {
            case Node3D: SwapScene(key, instance, _world3D, ref _current3D, ref _current3DKey); break;
            case Node2D: SwapScene(key, instance, _world2D, ref _current2D, ref _current2DKey); break;
            default:
                GD.PrintErr($"[SceneManager] '{key}' root is not Node3D or Node2D. Use SetUI instead.");
                instance.QueueFree();
                return;
        }

        EmitSignal(SignalName.WorldSceneChanged, key.ToString());
        GD.Print($"[SceneManager] World scene changed: '{key}'");
    }

    /// <summary>Frees a specific world scene if it is currently active.</summary>
    public void FreeScene(SceneNames key)
    {
        _preloadedScenes.Remove(key);
        _threadedLoads.Remove(key);

        if (_current3DKey == key)
        {
            FreeNode(ref _current3D);
            _current3DKey = null;
            GD.Print($"[SceneManager] Freed world 3D scene: '{key}'");
        }
        else if (_current2DKey == key)
        {
            FreeNode(ref _current2D);
            _current2DKey = null;
            GD.Print($"[SceneManager] Freed world 2D scene: '{key}'");
        }
        else
        {
            GD.PrintErr($"[SceneManager] FreeScene: '{key}' is not currently active.");
        }
    }

    /// <summary>
    /// Begins loading a world scene on a background thread.
    /// Call <see cref="ChangeScene"/> afterward, it will use the cached result if ready,
    /// or block until the load finishes if it is still in progress.
    /// </summary>
    public void PreloadScene(SceneNames key)
    {
        if (!Scenes.ContainsKey(key)) { GD.PrintErr($"[SceneManager] '{key}' is not registered."); return; }
        if (_preloadedScenes.ContainsKey(key) || _threadedLoads.ContainsKey(key))
        {
            GD.Print($"[SceneManager] '{key}' is already preloading or preloaded.");
            return;
        }

        var path = Scenes[key].Path;
        ResourceLoader.LoadThreadedRequest(path);
        _threadedLoads[key] = path;
        GD.Print($"[SceneManager] Preload started: '{key}'");
    }

    // ── Public API, UI ───────────────────────────────────────────────────────

    /// <summary>
    /// Shows a UI scene and sets it as current.
    /// If another UI is currently visible it is hidden first.
    /// If the target was previously shown it is reused; otherwise it is instantiated.
    /// </summary>
    public void SetUI(SceneNames key)
    {
        if (_currentUI == key) return;

        // Hide current if any.
        if (_currentUI.HasValue && _uiInstances.TryGetValue(_currentUI.Value, out var old))
        {
            (old as ISceneLifecycle)?.OnSceneRemoved();
            old.Set("visible", false);
        }

        // Reuse existing instance or instantiate fresh.
        if (!_uiInstances.TryGetValue(key, out var instance))
        {
            instance = InstanceFromKey<Node>(key);
            if (instance == null) return;
            _ui.AddChild(instance);
            _uiInstances[key] = instance;
        }

        instance.Set("visible", true);
        _currentUI = key;
        (instance as ISceneLifecycle)?.OnSceneAdded();
        EmitSignal(SignalName.UIChanged, key.ToString());
        GD.Print($"[SceneManager] UI set: '{key}'");
    }

    /// <summary>Hides the current UI scene. The instance is kept in memory.</summary>
    public void HideUI()
    {
        if (!_currentUI.HasValue) { GD.PrintErr("[SceneManager] HideUI: no UI is currently visible."); return; }

        if (_uiInstances.TryGetValue(_currentUI.Value, out var node))
        {
            (node as ISceneLifecycle)?.OnSceneRemoved();
            node.Set("visible", false);
        }

        EmitSignal(SignalName.UIHidden, _currentUI.Value.ToString());
        GD.Print($"[SceneManager] UI hidden: '{_currentUI.Value}'");
        _currentUI = null;
    }

    /// <summary>Frees a specific UI instance. Hides it first if it is currently visible.</summary>
    public void FreeUI(SceneNames key)
    {
        if (_currentUI == key)
        {
            _currentUI = null;
            EmitSignal(SignalName.UIHidden, key.ToString());
        }

        if (_uiInstances.TryGetValue(key, out var node))
        {
            (node as ISceneLifecycle)?.OnSceneRemoved();
            node.QueueFree();
            _uiInstances.Remove(key);
            _preloadedUI.Remove(key);
            GD.Print($"[SceneManager] UI freed: '{key}'");
        }
        else
        {
            GD.PrintErr($"[SceneManager] FreeUI: '{key}' has no active instance.");
        }
    }

    /// <summary>Frees all UI instances and clears current.</summary>
    public void ClearUI()
    {
        foreach (var (_, node) in _uiInstances)
        {
            (node as ISceneLifecycle)?.OnSceneRemoved();
            node.QueueFree();
        }

        _uiInstances.Clear();
        _preloadedUI.Clear();
        _currentUI = null;
        GD.Print("[SceneManager] UI cleared.");
    }

    /// <summary>Frees all active world scenes and clears all UI.</summary>
    public void FreeAll()
    {
        ClearUI();
        if (_current3D != null) { FreeNode(ref _current3D); _current3DKey = null; }
        if (_current2D != null) { FreeNode(ref _current2D); _current2DKey = null; }
        GD.Print("[SceneManager] Freed all scenes.");
    }

    /// <summary>
    /// Caches the PackedScene resource for a UI scene so the first
    /// <see cref="SetUI"/> instantiates without a disk read.
    /// </summary>
    /// <param name="loadHidden">If true, instantiates and adds the scene hidden to the UI layer, ready for <see cref="SetUI"/> to reuse.</param>
    public void PreloadUI(SceneNames key, bool loadHidden = false)
    {
        if (!Scenes.ContainsKey(key)) { GD.PrintErr($"[SceneManager] '{key}' is not registered."); return; }
        if (_preloadedUI.ContainsKey(key) || _uiInstances.ContainsKey(key)) return;
        var packed = GD.Load<PackedScene>(Scenes[key].Path);
        if (packed == null) { GD.PrintErr($"[SceneManager] PreloadUI failed: '{key}' at '{Scenes[key].Path}'"); return; }
        _preloadedUI[key] = packed;
        if (loadHidden)
        {
            var instance = packed.Instantiate<CanvasItem>();
            instance.Visible = false;
            _ui.AddChild(instance);
            _uiInstances[key] = instance;
        }
        GD.Print($"[SceneManager] UI preloaded: '{key}'");
    }

    // ── Queries ───────────────────────────────────────────────────────────────

    public SceneNames? CurrentWorld3D => _current3DKey;
    public SceneNames? CurrentWorld2D => _current2DKey;
    public SceneNames? CurrentUI      => _currentUI;

    /// <summary>Returns the active instance of any scene cast to T, or null.</summary>
    public T Get<T>(SceneNames key) where T : Node
    {
        if (_current3DKey == key) return _current3D as T;
        if (_current2DKey == key) return _current2D as T;
        if (_uiInstances.TryGetValue(key, out var node)) return node as T;
        return null;
    }

    // ── Internal helpers ──────────────────────────────────────────────────────

    private void SwapScene(SceneNames key, Node instance, Node parent, ref Node current, ref SceneNames? currentKey)
    {
        if (current != null)
            FreeNode(ref current);

        parent.AddChild(instance);
        current    = instance;
        currentKey = key;
        (instance as ISceneLifecycle)?.OnSceneAdded();
    }

    private static void FreeNode(ref Node node)
    {
        (node as ISceneLifecycle)?.OnSceneRemoved();
        node.QueueFree();
        node = null;
    }

    private T InstanceFromKey<T>(SceneNames key) where T : Node
    {
        if (!Scenes.ContainsKey(key)) { GD.PrintErr($"[SceneManager] '{key}' is not registered."); return null; }

        var path = Scenes[key].Path;
        PackedScene packed;

        if (_preloadedScenes.TryGetValue(key, out packed))
            _preloadedScenes.Remove(key); // world preload cache is single-use
        else if (_preloadedUI.TryGetValue(key, out packed))
            { } // UI cache is kept alive for reuse
        else if (_threadedLoads.ContainsKey(key))
        {
            GD.Print($"[SceneManager] Waiting for threaded load of '{key}'...");
            packed = (PackedScene)ResourceLoader.LoadThreadedGet(path);
            _threadedLoads.Remove(key);
        }
        else
            packed = GD.Load<PackedScene>(path);

        if (packed == null)
        {
            GD.PrintErr($"[SceneManager] Could not load scene: '{key}' at '{path}'");
            return null;
        }

        var instance = packed.Instantiate<T>();
        if (instance == null)
            GD.PrintErr($"[SceneManager] Scene root is not of type {typeof(T).Name}: '{key}'");

        return instance;
    }

    private static T GetOrCreateChild<T>(Node parent, string childName) where T : Node, new()
    {
        var existing = parent.GetNodeOrNull<T>(childName);
        if (existing != null) return existing;

        var node = new T { Name = childName };
        parent.AddChild(node);
        GD.Print($"[SceneManager] Created container '{childName}' under '{parent.Name}'");
        return node;
    }
}