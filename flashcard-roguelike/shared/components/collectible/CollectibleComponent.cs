using Godot;

public partial class CollectibleComponent : Node
{
	[Export] public bool IsCollectible = true;
	[Signal] public delegate void CollectedEventHandler(Node collector);

	public void Collect(Node collector = null)
	{
		if (!IsCollectible) return;
		if (collector is Player)
		{
			EmitSignal(SignalName.Collected, collector);
			GetParent()?.QueueFree();
		}
	}
}