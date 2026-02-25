using Godot;

public partial class Main : Node
{
    public override void _Ready()
    {
        CallDeferred(nameof(StartGame));
    }

    private void StartGame()
    {
        SceneManager.Instance.Navigate(SceneNames.MainMenu);
        // SceneManager.Instance.Preload(SceneNames.Dungeon);
    }

    // public override void _UnhandledInput(InputEvent @event)
	// {
	// 	if (@event.IsActionPressed("ui_cancel"))
    //     {
    //         SceneManager.Instance.SetVisibility(SceneNames.PauseMenu, true);
    //     }
    // }

    
}