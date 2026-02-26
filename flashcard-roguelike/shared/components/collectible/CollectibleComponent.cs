using Godot;
using System;

public partial class CollectibleComponent : Node
{
	[Export] public bool IsCollectible = true;
	[Signal] public delegate void CollectedEventHandler();

	/// Call this to collect the parent item. Optional `collector` is the node that collected it.
	public void Collect(Node collector = null)
	{
		if (!IsCollectible) return;
		EmitSignal(SignalName.Collected);
		OnCollected(collector);
		GetParent()?.QueueFree();
	}

	protected virtual void OnCollected(Node collector)
	{
		// Implement effects here, e.g. add to inventory, play sound, update UI
	}
}