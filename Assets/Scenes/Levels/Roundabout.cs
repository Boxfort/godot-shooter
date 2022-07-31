using Godot;
using System;

public class Roundabout : KinematicBody, Interactable
{
    public string InteractString => "SPIN";

    float rotationSpeed = 0.0f;
    const float rotationSpeedDecay = 2.0f;
    const float maxRotationSpeed = 10.0f;
    const float spinForce = 2.0f;

    public void Interact()
    {
        rotationSpeed = Mathf.Min(rotationSpeed + spinForce, maxRotationSpeed);
    }

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(float delta)
    {
        if (rotationSpeed <= 0)
            return;

        RotateY(rotationSpeed * delta);
        rotationSpeed = Mathf.Max(rotationSpeed - rotationSpeedDecay * delta, 0);
    }
}
