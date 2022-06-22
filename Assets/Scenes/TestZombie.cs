using Godot;
using System;
using System.Collections.Generic;

public class TestZombie : KinematicBody, Damageable
{
    [Export]
    PackedScene debugPoint;

    // NAVIGATION
    NavigationAgent navigationAgent;
    Vector3 lastTarget;
    Vector3[] path = new Vector3[0];
    bool targetReset = true;
    Dictionary<String, bool> canReachTarget = new Dictionary<String, bool>();
    float updatePathTime = 0.5f;
    float updatePathTimer = 1.0f;
    float detectionTickTime = 0.5f; 
    float detectionTickTimer = 0.5f; 
    Spatial player;


    int health = 25;

    // MOVEMENT
    float moveSpeed = 5;
    float rotationSpeed = 5.0f;
    Vector3 gravityVec = Vector3.Zero;
    Vector3 snap = Vector3.Zero;

    Vector3 knockbackVec = Vector3.Zero;

    AnimationTree animationTree;

    public int Health => health;

    public void TakeDamage(int damage)
    {
        GD.Print("Taking damage: " + damage);
        
    }

    public override void _Ready()
    {
        navigationAgent = GetNode<NavigationAgent>("NavigationAgent");
        player = GetNode<Spatial>("../../Player");
        animationTree = GetNode<AnimationTree>("AnimationTree");
    }

    public override void _Process(float delta)
    {
        if (updatePathTimer >= updatePathTime) 
        {
            SetTargetLocation(player.GlobalTransform.origin);
        } 
        else 
        {
            updatePathTimer += delta;
        }
    }

    private void DetectTarget(float delta)
    {
        
    }

    private void SetTargetLocation(Vector3 position) 
    {
        targetReset = true;
        updatePathTimer = 0;

        navigationAgent.SetTargetLocation(position);

        // Call 'GetNextLocation' to ensure the path exists. 
        navigationAgent.GetNextLocation();
        path = navigationAgent.GetNavPath();
    }

    public override void _PhysicsProcess(float delta)
    {
        HandleGravity(delta);
        var target = navigationAgent.GetNextLocation();

        if (target != lastTarget) 
        {
            var instance = debugPoint.Instance() as Spatial;
            GetTree().Root.AddChild(instance);
            var tf = instance.GlobalTransform;
            tf.origin = target;
            instance.GlobalTransform = tf;
            lastTarget = target;
        }

        LookAtSmooth(target, delta);

        if (!navigationAgent.IsTargetReached()) 
        {
            if (CanReachTarget(player.GlobalTransform.origin, player.GetInstanceId().ToString()))
            {
                var position = GlobalTransform.origin;
                var direction = target - GlobalTransform.origin;
                var velocity = MoveAndSlideWithSnap((direction.Normalized() * moveSpeed) + gravityVec, snap, Vector3.Up);
                animationTree.Set("parameters/IsRunning/blend_amount", 1);
            }
            else
            {
                animationTree.Set("parameters/IsRunning/blend_amount", 0);
                LookAtSmooth(player.GlobalTransform.origin, delta);
            }
        } 
        else
        {
                animationTree.Set("parameters/IsRunning/blend_amount", 0);
        } 
    }

    private bool CanReachTarget(Vector3 target, String id)
    {
        // We implement our own 'CanReachTarget' because the NavigationAgent implementation does not
        // conider the target 'reachable' if they are outside of the navigation mesh, but within the 
        // PathMaxDistance set by the agent. 

        bool canReach;

        // If the target has not been reset, fetch the value from cache, or calculate if doesn't exist. 
        if (!targetReset) 
        {
            if(!canReachTarget.TryGetValue(id, out canReach))
            {
                canReach = CanReachTargetInner(target);
            } 
        } 
        else 
        {
            // Clear targets to not fill up memory
            canReachTarget.Clear();
            targetReset = false;

            canReach = CanReachTargetInner(target);
        }

        canReachTarget[id] = canReach;
        return canReach;
    }

    private bool CanReachTargetInner(Vector3 target)
    {
        if (path.Length > 0) 
        {
            var lastPathNode = path[path.Length-1];
            return (lastPathNode.DistanceTo(target) <= navigationAgent.PathMaxDistance);
        } 
        else 
        {
            return false;
        }
    }

    void LookAtSmooth(Vector3 target, float delta, bool lockZ = true) 
    {
        Vector3 globalPosition = GlobalTransform.origin;
        Vector3 originalRotation = RotationDegrees;

        Transform desiredTransform = GlobalTransform.LookingAt(new Vector3(target.x, globalPosition.y, target.z), Vector3.Up).Rotated(Vector3.Up, Mathf.Deg2Rad(180));
        var desiredRotation = new Quat(GlobalTransform.basis).Slerp(new Quat(desiredTransform.basis).Normalized(), rotationSpeed*delta);
        GlobalTransform = new Transform(new Basis(desiredRotation), GlobalTransform.origin);

        if (lockZ) {
            originalRotation.x = RotationDegrees.x;
            originalRotation.y = RotationDegrees.y;
            RotationDegrees = originalRotation;
        }
    }

    void HandleGravity(float delta) 
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
