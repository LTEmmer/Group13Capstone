// SceneManager.cs
using Godot;
using System.Collections.Generic;
using System.Linq.Expressions;

/// <summary>
/// Defines the SceneNames dictionary for quick scene refrencing
/// </summary>
public enum SceneNames
{
    MainMenu,
    Dungeon,
    TestArea,
}

/// <summary>
/// Singleton that manages scene loading, and removing.
/// Scenes are kept as children of "/root/Main" and can be toggled
/// invisible rather than dequeued, allowing preloading and fast switching.
/// Use <b>GoTo()</b> for temporary transitions, <b>Preload</b> to load in the background,
/// and <b>ReplaceScene</b> to transition and free the previous scene in one call.
/// </summary>
/// <remarks>
/// SceneManager.Instance.ReplaceScene(SceneNames.Dungeon);
/// </remarks>
[GlobalClass]
public partial class SceneManager : Node
{
    public static SceneManager Instance { get; private set; }

    [Signal] public delegate void SceneChangedEventHandler(string sceneName);

	/// <summary>
	/// defines a dictionary with all used scenes defined by a name.
	/// </summary>
    public readonly Dictionary<SceneNames, SceneData> sceneDict = new()
    {
        { SceneNames.MainMenu, new SceneData("MainMenu", 	"res://game/ui/main_menu/main_menu.tscn")},
        { SceneNames.Dungeon,  new SceneData("Dungeon", 	"res://game/entity/dungeon_generator/dungeon_generator.tscn")},
        { SceneNames.TestArea, new SceneData("TestArea", 	"res://scenes/test_room.tscn")}
    };

    [Export] public NodePath MainPath { get; set; } = "/root/Main";

    private readonly Dictionary<SceneNames, Node> _loaded = new();
    private SceneNames? _active;
    private Node _mainNode;

    public override void _Ready()
    {
        Instance = this;
        CallDeferred(nameof(Init));
    }

    private void Init()
    {
        _mainNode = GetNode(MainPath);
        if (_mainNode == null)
            GD.PrintErr($"[SceneManager] Main node not found at '{MainPath}'");
    }

	/// <summary>
	/// Loades the scene defined by key but keeps it hidden
	/// </summary>
	/// <param name="key"></param>
    public void Preload(SceneNames key)
    {
        if (_loaded.ContainsKey(key)) return;

        var data   = sceneDict[key];
        var packed = GD.Load<PackedScene>(data.path);

        if (packed == null)
        {
            GD.PrintErr($"[SceneManager] Could not load '{data.path}'");
            return;
        }

        var instance = packed.Instantiate();
        instance.Set("visible", false);
        _mainNode.AddChild(instance);
        _loaded[key] = instance;
        GD.Print($"[SceneManager] Preloaded '{data.name}'");
    }

	/// <summary>
	/// Hides the current scene and opens the scene defined by key
	/// </summary>
	/// <param name="key"></param>
    public void GoTo(SceneNames key)
    {
        if (_active.HasValue && _loaded.TryGetValue(_active.Value, out var current))
        {
            current.Set("visible", false);
        }

        if (!_loaded.ContainsKey(key))
            Preload(key);

        if (_loaded.TryGetValue(key, out var next))
        {
            next.Set("visible", true);
            _active = key;
            EmitSignal(SignalName.SceneChanged, sceneDict[key].name);
            GD.Print($"[SceneManager] → '{sceneDict[key].name}'");
        }
    }

	/// <summary>
	/// frees the scene defined by key if it is loaded
	/// </summary>
	/// <param name="key"></param>
    public void Unload(SceneNames key)
    {
        if (_active == key)
        {
            GD.PrintErr($"[SceneManager] Can't unload the active scene '{key}'. Call GoTo() first.");
            return;
        }

        if (_loaded.TryGetValue(key, out var node))
        {
            node.QueueFree();
            _loaded.Remove(key);
            GD.Print($"[SceneManager] Unloaded '{sceneDict[key].name}'");
        }
    }

	/// <summary>
	/// Replaces the currently loaded scene with the scene from the given key
	/// </summary>
	/// <param name="key"></param>
	public void ReplaceScene(SceneNames key)
	{
    	SceneNames? previous = _active;

    	GoTo(key);

    	if (previous.HasValue && previous.Value != key)
        	Unload(previous.Value);
	}

	/// <summary>
	/// returns true if the given scene defined by the given key is loaded
	/// </summary>
	/// <param name="key"></param>
	/// <returns></returns>
    public bool IsLoaded(SceneNames key) => _loaded.ContainsKey(key);

	/// <summary>
	/// returns the active scene if there is any
	/// </summary>
    public SceneNames? ActiveScene => _active;

	 /// <summary>
	 /// Returns the root node of a loaded scene
	 /// </summary>
	 /// <typeparam name="T"></typeparam>
	 /// <param name="key"></param>
	 /// <returns></returns>
    public T GetScene<T>(SceneNames key) where T : Node =>
        _loaded.TryGetValue(key, out var n) ? n as T : null;
}