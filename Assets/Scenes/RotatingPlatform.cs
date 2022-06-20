using Godot;
using System;

public class RotatingPlatform : KinematicBody
{
    [Export]
    float speed = 1.0f;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(float delta)
    {
        Rotate(Vector3.Up, speed * delta);
    }
}
