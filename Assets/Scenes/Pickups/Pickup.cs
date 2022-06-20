using Godot;
using System;

public class Pickup : Area
{
    Spatial model;
    Vector3 startingPosition;

    const float bobSpeed = 3.0f;
    const float bobAmount = 0.3f;
    const float spinSpeed = 1.0f;
    float deltaCounter = 0.0f;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        model = GetNode<Spatial>("Model");
        startingPosition = model.GlobalTransform.origin;
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(float delta)
    {
        deltaCounter += delta;
        var transform = model.GlobalTransform;
        transform.origin.y = startingPosition.y + (Mathf.Sin(deltaCounter*bobSpeed) * bobAmount);
        model.GlobalTransform = transform;
        model.Rotate(Vector3.Up, spinSpeed * delta);
    }
}
