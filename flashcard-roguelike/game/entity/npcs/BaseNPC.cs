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
    [Export] private float _charsPerSecond = 10f;
    [Export] private bool _needsResponse = false;

    private bool _interactionTriggered = false;

    public override void Interact(Node caller)
    {
        if (_interactionTriggered) 
        {
          return;
        }

        DialogBoxManager.Instance.ShowDialog(_dialogText, _name, _npcIcon, _voice, _charsPerSecond, _needsResponse);
        //EmitSignal(nameof(OnInteraction));
        _interactionTriggered = true;
    }
}