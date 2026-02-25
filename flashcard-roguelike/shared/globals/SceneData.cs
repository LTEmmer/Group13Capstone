using Godot;
using System;

public partial class SceneData : Node
{
    public SceneData(String name, String path)
    {
        this.name = name;
        this.path = path;
    }
    public string name;
    public string path;
}
