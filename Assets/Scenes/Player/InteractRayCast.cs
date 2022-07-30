using Godot;
using System;

public class InteractRayCast : RayCast
{
    [Signal]
    delegate void OnRaycastEnter(string interactString);
    [Signal]
    delegate void OnRaycastExit();
    [Signal]
    delegate void OnBeginCarry();
    [Signal]
    delegate void OnEndCarry();

    const float throwForce = 20.0f;
    const float dropForce = 0.0f;
    const float carryDistance = 3.0f;
    const float carryDropDistance = 8.0f;

    bool wasColliding = false;
    string wasCollidingWith = "";

    bool isCarrying = false;
    public bool IsCarrying { get => isCarrying; }

    Carryable carrying;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {

    }

    public void DoInteract()
    {
        if (isCarrying)
        {
            DoDrop();
            return;
        }

        PhysicsBody collider = (PhysicsBody)GetCollider();
        if (collider != null)
        {
            if (collider is Carryable carryable)
            {
                carryable.OnCarry();
                isCarrying = true;
                carrying = carryable;
                wasColliding = false;
                wasCollidingWith = "";
                SetCarryablePlayerCollision(carryable, false);
                carryable.Connect("StopCarrying", this, nameof(AbandonCarryable));
                EmitSignal(nameof(OnRaycastExit));
                EmitSignal(nameof(OnBeginCarry));
            }

            if (collider is Interactable interactable)
            {
                interactable.Interact();
            }
        }
    }

    private void SetCarryablePlayerCollision(Carryable carryable, bool shouldCollide)
    {
        carryable.SetCollisionMaskBit(1, shouldCollide);
        carryable.SetCollisionMaskBit(2, shouldCollide);
        carryable.SetCollisionLayerBit(0, shouldCollide);
        carryable.SetCollisionLayerBit(5, !shouldCollide);
    }

    private void AbandonCarryable()
    {
        isCarrying = false;
        carrying = null;
        EmitSignal(nameof(OnEndCarry));
    }

    public void DoDrop()
    {
        if (!isCarrying)
            return;

        SetCarryablePlayerCollision(carrying, true);
        isCarrying = false;
        carrying.OnDrop(-GlobalTransform.basis.z.Normalized(), dropForce);
        carrying = null;
        EmitSignal(nameof(OnEndCarry));
    }

    public void DoThrow()
    {
        if (!isCarrying)
            return;

        SetCarryablePlayerCollision(carrying, true);
        isCarrying = false;
        carrying.OnThrow(-GlobalTransform.basis.z.Normalized(), throwForce);
        carrying = null;
        EmitSignal(nameof(OnEndCarry));
    }

    public override void _PhysicsProcess(float delta)
    {
        if (isCarrying)
        {
            HandleCarry();
            return;
        }

        PhysicsBody collider = (PhysicsBody)GetCollider();
        if (collider != null)
        {
            string colliderRID = collider.GetRid().ToString();

            if (wasColliding == false || wasCollidingWith != colliderRID)
            {
                // We're colliding with a new object!
                if (collider is Interactable interactable)
                {
                    if (wasColliding)
                        EmitSignal(nameof(OnRaycastExit));

                    EmitSignal(nameof(OnRaycastEnter), interactable.InteractString);

                    wasColliding = true;
                    wasCollidingWith = colliderRID;
                }
            }
        }
        else if (wasColliding)
        {
            wasColliding = false;
            wasCollidingWith = "";
            EmitSignal(nameof(OnRaycastExit));
        }
    }

    private void HandleCarry()
    {
        if (carrying == null)
        {
            isCarrying = false;
            return;
        }

        Transform carryTargetTransform = GlobalTransform;
        carryTargetTransform.origin += (-GlobalTransform.basis.z.Normalized() * carryDistance);

        Vector3 directionToTarget = carrying.GlobalTransform.origin.DirectionTo(carryTargetTransform.origin);
        float distanceToTarget = carrying.GlobalTransform.origin.DistanceTo(carryTargetTransform.origin);

        if (distanceToTarget > 0.1f)
        {
            carrying.MoveAndSlide(directionToTarget * (distanceToTarget * 30), Vector3.Up, floorMaxAngle: 1.0f);
        }

        if (distanceToTarget > carryDropDistance)
        {
            DoDrop();
        }
    }
}
