using Godot;
using System;

public class MovingPlatform : KinematicBody
{
    [Export]
    Vector3 start;

    [Export]
    Vector3 end;

    [Export]
    float speed = 5.0f;

    [Export]
    float delayTime = 1.0f;

    float delay = 0f;
    bool delaying = false;
    bool returning = false;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(float delta)
    {
        if (true)
        {
            var transform = GlobalTransform;

            transform.origin = transform.origin.MoveToward(returning ? end : start, speed * delta);

            if (transform.origin.IsEqualApprox(start) || transform.origin.IsEqualApprox(end))
            {
                delaying = true;
                returning = !returning;
            }

            GlobalTransform = transform;
        }
        else
        {
            delay += delta;
            if( delay >= delayTime) 
            {
                delay = 0;
                delaying = false;
            }
        }
    }
}
