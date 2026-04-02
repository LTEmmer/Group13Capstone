using Godot;

/// <summary>
/// Hosts a full-bleed torn-paper TextureRect behind <c>MarginContainer</c> content.
/// Minimum size follows the margin + children so the paper scales uniformly to fit the layout.
/// </summary>
public partial class TornPaperMenuPanel : Control
{
	public override Vector2 _GetMinimumSize()
	{
		var margin = GetNodeOrNull<MarginContainer>("MarginContainer");
		return margin?.GetCombinedMinimumSize() ?? Vector2.Zero;
	}
}
