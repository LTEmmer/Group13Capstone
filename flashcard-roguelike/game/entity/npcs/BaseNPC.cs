using System;
using Godot;

public partial class BaseNPC : Interactable 
{
    [Signal]
    public delegate void OnInteractionEventHandler();
    
    [Export] private string _name;
    [Export] private string _dialogText;
    [Export] private Texture2D _npcIcon;
    [Export] private AudioStream _voice;

    private bool _interactionTriggered = false;

    public override void Interact(Node caller)
    {
      DialogBoxManager.Instance.ShowDialog(_dialogText, _name, _npcIcon, _voice);
		  //EmitSignal(nameof(OnInteraction));
		  _interactionTriggered = true;
    }
}