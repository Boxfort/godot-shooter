using Godot;
using System;

public class DebugCollision : Spatial
{
    float lifetime = 5.0f;
    float lifetimeCounter = 0f;
    

    public override void _Ready()
    {
        
    }

    public override void _Process(float delta)
    {
        lifetimeCounter += delta;

        if(lifetimeCounter >= lifetime) 
        {
            QueueFree();
        }
    }
}
