using Godot;
using System;

public partial class BattleTransition : CanvasLayer
{
    [Export] public int SliceCount = 5;
    [Export] public float SliceDuration = 1f;
    [Export] public float StaggerDelay = 0.1f;

    public override void _Ready()
    {
        Layer = 10; // Ensure this is above all other UI elements
    }

    // Call this to slam the slices shut, onComplete will be called after all slices have finished sliding in
    public void Cover(Action onComplete = null)
    {
        // Capture the current screen as a texture and determine slice sizes
        ImageTexture screenTexture = CaptureScreen();
        Vector2 screenSize = GetViewport().GetVisibleRect().Size;
        float sliceHeight = screenSize.Y / SliceCount;

        // Create tween to animate slicens, parallel to handle all slices
        Tween tween = CreateTween();
        tween.SetParallel(true);

        for (int i = 0; i < SliceCount; i++)
        {
            // 
            float direction = (i % 2 == 0) ? 1f : -1f;

            // Create the slice and atlas to show the correct portion of the screen
            TextureRect slice = new TextureRect();
            AtlasTexture atlas = new AtlasTexture();

            // Set the atlas to show the correct horizontal slice of the captured screen
            atlas.Atlas = screenTexture;
            atlas.Region = new Rect2(0, i * sliceHeight, screenSize.X, sliceHeight);

            // Configure the slice to use the atlas and size it to cover the screen width
            slice.Texture = atlas;
            slice.Size = new Vector2(screenSize.X, sliceHeight);
            AddChild(slice);

            // Start off-screen, slide in to cover
            slice.Position = new Vector2(-direction * screenSize.X, i * sliceHeight);

            // Animate the slice sliding in from the left or right
            tween.TweenProperty(slice, "position:x", 0f, SliceDuration)
                 .SetDelay(i * StaggerDelay)
                 .SetTrans(Tween.TransitionType.Quad)
                 .SetEase(Tween.EaseType.Out);
        }

        // Calculate total duration to know when to call onComplete after all slices have slid in
        float totalDuration = SliceDuration + (SliceCount - 1) * StaggerDelay;

        // Chain a callback after all slices have finished sliding in
        // Callable.From is used to convert the lambda to a Callable that can be used in the tween
        tween.Chain().TweenCallback(Callable.From(() => onComplete?.Invoke())).SetDelay(totalDuration);
    }

    // Call this when done to reveal screen, onComplete will be called after all slices have finished sliding off
    public void Reveal(Action onComplete = null)
    {
        // Get the current screen size for sliding slices off
        Vector2 screenSize = GetViewport().GetVisibleRect().Size;

        // Tween to animate slices sliding off, parallel to handle all slices 
        Tween tween = CreateTween();
        tween.SetParallel(true);

        // Slide each slice off in the opposite direction it came from
        var slices = GetChildren();
        for (int i = 0; i < slices.Count; i++)
        {
            float direction = (i % 2 == 0) ? 1f : -1f;

            // Animate the slice sliding off the screen to the left or right
            tween.TweenProperty(slices[i], "position:x", Variant.From(direction * screenSize.X), SliceDuration)
                 .SetDelay(i * StaggerDelay)
                 .SetTrans(Tween.TransitionType.Quad)
                 .SetEase(Tween.EaseType.In);
        }

        // Calculate total duration to know when to call onComplete after all slices have slid off
        float totalDuration = SliceDuration + (SliceCount - 1) * StaggerDelay;

        // Chain a calback to free all slices and call onComplete after all slices have finished sliding off
        tween.Chain().TweenCallback(Callable.From(() =>
        {
            foreach (Node child in GetChildren())
                child.QueueFree();

            onComplete?.Invoke();
        })).SetDelay(totalDuration);
    }

    // Helper method to capture the current screen as a texture 
    private ImageTexture CaptureScreen()
    {
        Image image = GetViewport().GetTexture().GetImage();
        return ImageTexture.CreateFromImage(image);
    }

    // Example input to trigger the transition for testing
    public override void _Input(InputEvent @event)
    {
        if (@event.IsActionPressed("ui_accept")) // spacebar by default
        {
            Cover(() =>
            {
                GD.Print("Covered!");
                Reveal(() => GD.Print("Revealed!"));
            });
        }
    }
}