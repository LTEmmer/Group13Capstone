using Godot;
using System;

[GlobalClass]
public abstract partial class Interactable : Node3D
{
    public override void _Ready()
    {
        AddToGroup("interactable");
    }

    public abstract void Interact(Node caller);
    public virtual void HoverStart(Node caller){}
    public virtual void HoverEnd(Node caller){}
}