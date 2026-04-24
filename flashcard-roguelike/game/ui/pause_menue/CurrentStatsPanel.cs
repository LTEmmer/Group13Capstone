using Godot;

public partial class CurrentStatsPanel : Control
{
	private VBoxContainer _statsContainer;
	private const int StatFontSize = 22;

	public override void _Ready()
	{
		_statsContainer = GetNodeOrNull<VBoxContainer>(
			"StatsPanel/MarginContainer/VBoxContainer/ScrollContainer/StatsContainer"
		);
	}

	public void PopulateStats()
	{
		if (_statsContainer == null)
		{
			return;
		}

		foreach (Node child in _statsContainer.GetChildren())
		{
			_statsContainer.RemoveChild(child);
			child.QueueFree();
		}

		foreach (var stat in TaloTelemetry.GetSessionStats())
		{
			var row = new HBoxContainer();
			row.AddThemeConstantOverride("separation", 8);

			var labelNode = new Label
			{
				Text = stat.Label,
				SizeFlagsHorizontal = SizeFlags.ExpandFill,
				AutowrapMode = TextServer.AutowrapMode.WordSmart,
			};

			labelNode.AddThemeFontSizeOverride("font_size", StatFontSize);
			row.AddChild(labelNode);

			var valueNode = new Label
			{
				Text = stat.Value,
				HorizontalAlignment = HorizontalAlignment.Right,
			};
			
			valueNode.AddThemeFontSizeOverride("font_size", StatFontSize);
			row.AddChild(valueNode);

			_statsContainer.AddChild(row);
		}
	}
}
