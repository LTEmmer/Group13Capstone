using Godot;

public partial class Main : Node
{
    public override void _Ready()
    {
        CallDeferred(nameof(StartGame));
    }

    private void StartGame()
    {
        SceneManager.Instance.GoTo(SceneNames.MainMenu);
        // SceneManager.Instance.Preload(SceneNames.Dungeon);
    }
}