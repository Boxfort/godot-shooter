using Godot;
using System;

public class WoodGibs : Spatial
{
    Particles particles1;
    Particles particles2;

    float destroyTimer = 0.0f;
    const float destroyTime = 2.0f;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        particles1 = GetNode<Particles>("Particles");
        particles2 = GetNode<Particles>("Particles2");

        particles1.Emitting = true;
        particles2.Emitting = true;
    }

    public override void _Process(float delta)
    {
        if (destroyTimer < destroyTime)
        {
            destroyTimer += delta;
        }
        else
        {
            QueueFree();
        }
    }
}
