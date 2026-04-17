using Godot;
using System.Threading.Tasks;

public partial class SubViewport2: SubViewport
{
    public override async void _Ready()
    {
        await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
        await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);

        await SaveImage();
    }

    public async Task SaveImage(string path = "res://assets/sprites/item_sprites/capture.png")
    {
        // Wait until the frame finishes rendering
        await ToSignal(RenderingServer.Singleton, RenderingServer.SignalName.FramePostDraw);

        Image img = GetTexture().GetImage();
        img.SavePng(path);

        GD.Print("Saved viewport image to: " + path);
    }
}