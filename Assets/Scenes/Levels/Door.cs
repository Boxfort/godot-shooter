using Godot;
using System;

public class Door : KinematicBody, Interactable
{
    [Export]
    Vector3 openedRotation = new Vector3(0, 90, 0);
    [Export]
    Vector3 closedRotation = new Vector3(0, 0, 0);

    [Export]
    string openInteractString = "OPEN";

    [Export]
    string closeInteractString = "CLOSE";

    Vector3 desiredRotation;

    float doorTime = 1f;
    bool isOpen = false;

    Tween tween;

    public string InteractString => isOpen ? closeInteractString : openInteractString;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        desiredRotation = closedRotation;
        tween = GetNode<Tween>("Tween");
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(float delta)
    {
    }

    public void Toggle()
    {
        if (desiredRotation == closedRotation)
        {
            desiredRotation = openedRotation;
            isOpen = true;
        }
        else
        {
            desiredRotation = closedRotation;
            isOpen = false;
        }

        tween.InterpolateProperty(this, "rotation_degrees", RotationDegrees, desiredRotation, doorTime, Tween.TransitionType.Bounce, Tween.EaseType.Out);
        tween.Start();
    }

    public void Interact()
    {
        Toggle();
    }
}
