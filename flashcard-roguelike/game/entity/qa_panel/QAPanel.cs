using Godot;
using System;
using System.ComponentModel;

public partial class QAPanel : Area3D
{
    [Signal]
    public delegate void OnPanelHitEventHandler(QAPanel panel);

    [Export] public MeshInstance3D TargetMesh;

    
    private Label3D _label;
    private Flashcard _card;
    private bool isActive = true;

    public override void _Ready()
    {
        _label = GetNode<Label3D>("Label3D");
    }

    public void OnHit()
    {
        if (!isActive) return;

        isActive = false;
        GD.Print("QAPanel was hit!");
        EmitSignal("OnPanelHit", this);
    }

    public void SetPanelText(string text)
    {
        if (_label != null)
        {
            _label.Text = text;
        }
        else
        {
            GD.PrintErr("Error: Label3D node not found in QAPanel.");
        }
    }

    public string GetPanelText()
    {
        return _label != null ? _label.Text : string.Empty;
    }

    public Flashcard GetCard()
    {
        return _card;
    }

    public void AssignFlashcard(Flashcard card)
    {
        _card = card;
    }

    public void ShowResult(bool correct)
    {
        var mat = TargetMesh.GetSurfaceOverrideMaterial(0).Duplicate() as StandardMaterial3D;
        mat.EmissionEnabled = true;
        mat.Emission = correct ? new Color(0, 1, 0) : new Color(1, 0, 0); // green or red
        mat.EmissionEnergyMultiplier = 1f;
        TargetMesh.SetSurfaceOverrideMaterial(0, mat);
    }

    public void GlowSelected()
    {
        var mat = TargetMesh.GetSurfaceOverrideMaterial(0).Duplicate() as StandardMaterial3D;
        mat.EmissionEnabled = true;
        mat.Emission = new Color(1, 1, 0); // yellow glow
        mat.EmissionEnergyMultiplier = 1f;
        TargetMesh.SetSurfaceOverrideMaterial(0, mat);
    }

    public void SetAndPlaySound(AudioStream sound)
    {
        AudioStreamPlayer3D player = GetNode<AudioStreamPlayer3D>("AudioStreamPlayer3D");
        player.Stream = sound;
        player.Play();
    }
}
