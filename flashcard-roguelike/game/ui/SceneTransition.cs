using Godot;
using System;

public static class SceneTransition
{
    private const float DefaultDuration = 0.8f;

    public static void FadeOut(Node context, Action onComplete, float duration = DefaultDuration)
    {
        var layer = new CanvasLayer { Layer = 100 };
        context.AddChild(layer);

        var overlay = new ColorRect { Color = new Color(0, 0, 0, 0) };
        overlay.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);
        layer.AddChild(overlay);

        var tween = context.CreateTween();
        tween.SetPauseMode(Tween.TweenPauseMode.Process);
        tween.TweenProperty(overlay, "color:a", 1.0f, duration);
        if (onComplete != null)
            tween.TweenCallback(Callable.From(onComplete));
    }

    public static void FadeIn(Node context, float duration = DefaultDuration)
    {
        var layer = new CanvasLayer { Layer = 100 };
        context.AddChild(layer);

        var overlay = new ColorRect { Color = new Color(0, 0, 0, 1) };
        overlay.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);
        layer.AddChild(overlay);

        var tween = context.CreateTween();
        tween.SetPauseMode(Tween.TweenPauseMode.Process);
        tween.TweenProperty(overlay, "color:a", 0.0f, duration);
        tween.TweenCallback(Callable.From(layer.QueueFree));
    }
}
