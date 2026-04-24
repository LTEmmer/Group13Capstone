using Godot;
using System;
using System.Collections.Generic;

public partial class Minimap : Control
{
	private const int CellSize = 30;
	private const int CellGap = 6;
	private const int CellTotal = CellSize + CellGap;

	private static readonly Color ColEntrance = new(0.2f, 0.8f, 0.2f); // green
	private static readonly Color ColCombat = new(0.8f, 0.2f, 0.2f); // red
	private static readonly Color ColEvent = new(0.2f, 0.4f, 0.9f); // blue
	private static readonly Color ColTreasure = new(0.9f, 0.8f, 0.1f); // yellow
	private static readonly Color ColExit = new(0.6f, 0.1f, 0.8f); // purple
	private static readonly Color ColUnvisited = new(0.15f, 0.15f, 0.15f); // dark grey

	private DungeonGraph _graph;
	private readonly Dictionary<int, ColorRect> _rects = new();
	private readonly HashSet<int> _visited = new();
	private int _currentId = -1;
	private ColorRect _highlight;
	private double _blinkTimer = 0;

	public override void _Process(double delta)
	{
		if (_highlight == null || _currentId < 0) 
		{
			return;
		}

		_blinkTimer += delta;

		// Pulse the highlight alpha between 30% and 100% using a sine wave
		float alpha = (float)(0.3 + 0.7 * Math.Abs(Math.Sin(_blinkTimer * Math.PI)));
		_highlight.Modulate = new Color(1f, 1f, 1f, alpha);
	}

	public override void _EnterTree()
	{
		// Subscribe every time the node enters the tree — including after player reparenting between rooms
		if (CurrentRoomManager.Instance != null)
		{
			CurrentRoomManager.Instance.RoomChanged += OnRoomChanged;
			CurrentRoomManager.Instance.GraphChanged += OnGraphChanged;
			// If the graph is already built (re-entry after reparent), sync to the current room immediately
			if (_graph != null)
				OnRoomChanged(CurrentRoomManager.Instance.CurrentRoomId);
			else if (CurrentRoomManager.Instance.GraphRef != null)
				Initialize(CurrentRoomManager.Instance.GraphRef);
		}
	}

	private void OnGraphChanged()
	{
		// Reinitialize whenever a new dungeon is generated (quick restart or floor transition)
		if (CurrentRoomManager.Instance?.GraphRef != null)
		{
			Initialize(CurrentRoomManager.Instance.GraphRef);
		}
	}

	// Called by HUD after DungeonGenerator has finished building the graph
	public void Initialize(DungeonGraph graph)
	{
		_graph = graph;
		BuildVisuals();
		// Reflect the current room immediately in case the signal already fired
		if (CurrentRoomManager.Instance != null)
		{
			OnRoomChanged(CurrentRoomManager.Instance.CurrentRoomId);
		}

	}

	private void BuildVisuals()
	{
		// Clear any existing children from a previous generation
		foreach (Node child in GetChildren())
		{
			child.QueueFree();
		}
		_rects.Clear();
		_visited.Clear();
		_currentId = -1;
		_highlight = null;

		// Shift grid so the minimum row maps to y=0 in control space (handles negative branch rows)
		int minRow = 0;
		foreach (DungeonRoom room in _graph.Rooms)
		{
			if (room.MinimapGridPosition.Y < minRow)
			{
				minRow = room.MinimapGridPosition.Y;
			}
		}

		foreach (DungeonRoom room in _graph.Rooms)
		{
			int col = room.MinimapGridPosition.X;
			int row = room.MinimapGridPosition.Y - minRow;
			ColorRect rect = new ColorRect
			{
				CustomMinimumSize = new Vector2(CellSize, CellSize),
				Size = new Vector2(CellSize, CellSize),
				Position = new Vector2(col * CellTotal, row * CellTotal),
				Color = ColUnvisited
			};
			AddChild(rect);
			_rects[room.Id] = rect;
		}

		// Highlight overlay drawn on top of the current room cell
		_highlight = new ColorRect
		{
			CustomMinimumSize = new Vector2(CellSize + 4, CellSize + 4),
			Size = new Vector2(CellSize + 4, CellSize + 4),
			Color = new Color(1f, 1f, 1f, 0.6f),
			Visible = false
		};
		AddChild(_highlight); // Added last so it renders above the room cells

		// Set minimum size so a PanelContainer parent can auto-fit around the content
		float maxX = 0f, maxY = 0f;
		foreach (ColorRect r in _rects.Values)
		{
			if (r.Position.X + CellSize > maxX) maxX = r.Position.X + CellSize;
			if (r.Position.Y + CellSize > maxY) maxY = r.Position.Y + CellSize;
		}
		CustomMinimumSize = new Vector2(maxX + 4f, maxY + 4f);

		QueueRedraw(); // Trigger _Draw to render connection lines after rooms are positioned
	}

	public override void _Draw()
	{
		if (_graph == null) return;

		// Draw lines between connected rooms. Because _Draw runs on the parent before children render,
		// lines appear behind the room squares automatically.
		Color lineColor = new Color(0.5f, 0.5f, 0.5f, 0.6f);
		Vector2 halfCell = new Vector2(CellSize / 2f, CellSize / 2f);

		foreach (DungeonRoom room in _graph.Rooms)
		{
			if (!_rects.TryGetValue(room.Id, out ColorRect fromRect)) 
			{
				continue;
			}

			Vector2 from = fromRect.Position + halfCell;

			foreach (int outgoing in room.OutgoingConnections)
			{
				if (!_rects.TryGetValue(outgoing, out ColorRect toRect)) 
				{
					continue;
				}

				Vector2 to = toRect.Position + halfCell;
				DrawLine(from, to, lineColor, 2f);
			}
		}
	}

	private void OnRoomChanged(int newId)
	{
		// Color the previously current room with its type color (now visited)
		if (_currentId >= 0 && _rects.TryGetValue(_currentId, out ColorRect prev))
		{
			_visited.Add(_currentId);
			prev.Color = RoomColor(_graph.GetRoom(_currentId).RoomType);
		}

		_currentId = newId;

		// Color the new current room and move the highlight overlay onto it
		if (_rects.TryGetValue(newId, out ColorRect cur))
		{
			_visited.Add(newId);
			cur.Color = RoomColor(_graph.GetRoom(newId).RoomType);
			_highlight.Position = cur.Position - new Vector2(2f, 2f);
			_highlight.Visible = true;
		}
	}

	private static Color RoomColor(RoomTypes type) => type switch
	{
		RoomTypes.Entrance => ColEntrance,
		RoomTypes.Combat   => ColCombat,
		RoomTypes.Event    => ColEvent,
		RoomTypes.Treasure => ColTreasure,
		RoomTypes.Exit     => ColExit,
		_                  => ColUnvisited
	};

	public override void _ExitTree()
	{
		// Unsubscribe to avoid dangling signal connections
		if (CurrentRoomManager.Instance != null)
		{
			CurrentRoomManager.Instance.RoomChanged -= OnRoomChanged;
			CurrentRoomManager.Instance.GraphChanged -= OnGraphChanged;
		}
	}
}
