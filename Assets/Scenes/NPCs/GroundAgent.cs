using Godot;
using System;
using System.Collections.Generic;

public abstract class GroundAgent : KinematicBody
{
    [Export]
    private PackedScene debugPoint;

    protected NavigationAgent navigationAgent;
    protected Area detectionArea;
    protected Spatial target;

    protected Vector3 lastTarget;
    protected Vector3[] currentPath = new Vector3[0];
    private Dictionary<String, bool> canReachTarget = new Dictionary<String, bool>();

    protected bool targetPositionReset = false;
    protected bool debug = false;

    // MOVEMENT
    protected abstract float MoveSpeed { get; set; }
    protected abstract float RotationSpeed { get; set; }
    protected Vector3 currentVelocity = Vector3.Zero;
    protected Vector3 gravityVec = Vector3.Zero;
    protected Vector3 snap = Vector3.Zero;

    public override void _Ready()
    {
        navigationAgent = GetNode<NavigationAgent>("NavigationAgent");
        detectionArea = GetNode<Area>("DetectionArea");

        detectionArea.Connect("body_entered", this, nameof(OnBodyEnteredDectection));
        detectionArea.Connect("body_exited", this, nameof(OnBodyExitedDectection));
    }

    protected abstract void OnBodyEnteredDectection(PhysicsBody body);

    protected abstract void OnBodyExitedDectection(PhysicsBody body);

    protected void SetTarget(Spatial newTarget)
    {
        target = newTarget;

        UpdateTargetPosition();
    }

    protected void UpdateTargetPosition()
    {
        targetPositionReset = true;

        if (target != null)
        {
            navigationAgent.SetTargetLocation(target.GlobalTransform.origin);
        }

        // Call 'GetNextLocation' to ensure the path exists. 
        navigationAgent.GetNextLocation();
        currentPath = navigationAgent.GetNavPath();
    }

    public override void _PhysicsProcess(float delta)
    {
        currentVelocity = Vector3.Zero;
        HandleGravity(delta);

        if (target != null)
        {
            GotToTarget(delta);
        }
    }

    protected void GotToTarget(float delta)
    {
        Vector3 targetLocation = navigationAgent.GetNextLocation();

        if (targetLocation != lastTarget)
        {
            if (debug)
            {
                InstantiateDebugPoint(targetLocation);
            }
            lastTarget = targetLocation;
        }

        LookAtSmooth(targetLocation, delta);

        if (!navigationAgent.IsTargetReached())
        {
            if (CanReachTarget(target.GlobalTransform.origin, target.GetInstanceId().ToString()))
            {
                var position = GlobalTransform.origin;
                var direction = targetLocation - GlobalTransform.origin;
                currentVelocity = MoveAndSlideWithSnap((direction.Normalized() * MoveSpeed) + gravityVec, snap, Vector3.Up);
            }
        }
    }

    private void InstantiateDebugPoint(Vector3 position)
    {
        var instance = debugPoint.Instance() as Spatial;
        GetTree().Root.AddChild(instance);
        var debugTransform = instance.GlobalTransform;
        debugTransform.origin = position;
        instance.GlobalTransform = debugTransform;
        lastTarget = position;
    }

    protected bool CanReachTarget(Vector3 target, String id)
    {
        // We implement our own 'CanReachTarget' because the NavigationAgent implementation does not
        // conider the target 'reachable' if they are outside of the navigation mesh, but within the 
        // PathMaxDistance set by the agent. 

        bool canReach;

        // If the target has not been reset, fetch the value from cache, or calculate if doesn't exist. 
        if (!targetPositionReset)
        {
            if (!canReachTarget.TryGetValue(id, out canReach))
            {
                canReach = CanReachTargetInner(target);
            }
        }
        else
        {
            // Clear targets to not fill up memory
            canReachTarget.Clear();
            targetPositionReset = false;

            canReach = CanReachTargetInner(target);
        }

        canReachTarget[id] = canReach;
        return canReach;
    }

    private bool CanReachTargetInner(Vector3 target)
    {
        if (currentPath.Length > 0)
        {
            var lastPathNode = currentPath[currentPath.Length - 1];
            return (lastPathNode.DistanceTo(target) <= navigationAgent.PathMaxDistance);
        }
        else
        {
            return false;
        }
    }

    protected void LookAtSmooth(Vector3 target, float delta, bool lockZ = true)
    {
        Vector3 globalPosition = GlobalTransform.origin;
        Vector3 originalRotation = RotationDegrees;

        Transform desiredTransform = GlobalTransform.LookingAt(new Vector3(target.x, globalPosition.y, target.z), Vector3.Up).Rotated(Vector3.Up, Mathf.Deg2Rad(180));
        var desiredRotation = new Quat(GlobalTransform.basis).Slerp(new Quat(desiredTransform.basis).Normalized(), RotationSpeed * delta);
        GlobalTransform = new Transform(new Basis(desiredRotation), GlobalTransform.origin);

        if (lockZ)
        {
            originalRotation.x = RotationDegrees.x;
            originalRotation.y = RotationDegrees.y;
            RotationDegrees = originalRotation;
        }
    }

    protected void HandleGravity(float delta)
    {
        if (IsOnFloor())
        {
            snap = -GetFloorNormal();
            gravityVec = Vector3.Zero;
        }
        else
        {
            snap = Vector3.Down;
            gravityVec += Vector3.Down * 40.0f * delta;
        }
    }

}