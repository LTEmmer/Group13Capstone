using Godot;
using System;
using System.Numerics;
using Vector3 = Godot.Vector3;
using Vector2 = Godot.Vector2;

public partial class BattleTransition : CanvasLayer
{
    [Export] public int SliceCount = 5;
    [Export] public float SliceDuration = 1f;
    [Export] public float StaggerDelay = 0.15f;
    [Export] public Player Player;
    [Export] public EnemyExample Enemy;

    public override void _Ready()
    {
        Layer = 10; // Ensure this is above all other UI elements
    }

    // Call this to slam the slices shut, onComplete will be called after all slices have finished sliding in
    public void Cover(Vector3 targetPos, Player player, Action onComplete = null)
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

        // Chain a callback after all slices have finished sliding in
        // Callable.From is used to convert the lambda to a Callable that can be used in the tween
        tween.Chain().TweenCallback(Callable.From(() => 
        {
            // Look at the enemy to set the correct view for the transition
            player.LookAt(targetPos, Vector3.Up);
            onComplete?.Invoke();
        })).SetDelay(.5f); // Small delay to ensure all slices have fully covered the screen before calling onComplete
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

        // Chain a calback to free all slices and call onComplete after all slices have finished sliding off
        tween.Chain().TweenCallback(Callable.From(() =>
        {
            foreach (Node child in GetChildren())
                child.QueueFree();


            onComplete?.Invoke();
        })).SetDelay(.25f); // Small delay to ensure all slices have fully slid off before freeing and calling onComplete
    }

    // Method for taking the screen texture and sliding out, used for going out of battle
    public void SliceOut() 
    {
        // Get the texture of the current screen to use for the slices
        ImageTexture screenTexture = CaptureScreen();
        Vector2 screenSize = GetViewport().GetVisibleRect().Size;
        float sliceHeight = screenSize.Y / SliceCount;

        // Create tween to animate slices, parallel to handle all slices
        Tween tween = CreateTween();
        tween.SetParallel(true);


        // Slide each slice off in a random direction, alternating left and right
        for (int i = 0; i < SliceCount; i++)
        {
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

            // Start on-screen, slide out to left or right
            slice.Position = new Vector2(0, i * sliceHeight);

            // Animate the slice sliding off the screen to the left or right
            tween.TweenProperty(slice, "position:x", Variant.From(direction * screenSize.X), SliceDuration)
                 .SetDelay(i * StaggerDelay)
                 .SetTrans(Tween.TransitionType.Quad)
                 .SetEase(Tween.EaseType.In);
        }

        // Calculate total duration to know when to call onComplete after all slices have slid off
        float totalDuration = SliceDuration + (SliceCount - 1) * StaggerDelay;

        // Chain a callback to free all slices 
        tween.Chain().TweenCallback(Callable.From(() =>
        {
            foreach (Node child in GetChildren())
                child.QueueFree();
        })).SetDelay(totalDuration);
    }

    // Helper method to capture the current screen as a texture 
    private ImageTexture CaptureScreen()
    {
        Image image = GetViewport().GetTexture().GetImage();
        return ImageTexture.CreateFromImage(image);
    }
}