using System;
using Godot;

public partial class BaseNPC : Interactable 
{
    [Signal]
    public delegate void OnInteractionEventHandler();

    private bool _interactionTriggered = false;

    public override void Interact(Node caller)
    {

		EmitSignal(nameof(OnInteraction));
		_interactionTriggered = true;
    }
}