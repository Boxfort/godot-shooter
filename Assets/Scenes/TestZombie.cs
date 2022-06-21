using Godot;
using System;
using System.Collections.Generic;

public class TestZombie : KinematicBody, Damageable
{
    NavigationAgent navigationAgent;
    Vector3[] path = new Vector3[0];
    int pathNode = 0;

    Spatial player;

    float moveSpeed = 10;

    int health = 25;

    public int Health => health;

    public void TakeDamage(int damage)
    {
        GD.Print("Taking damage: " + damage);
    }

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        navigationAgent = GetNode<NavigationAgent>("NavigationAgent");
        player = GetNode<Spatial>("../../Player");
    }

    float updatePathTime = 1f;
    float updatePathTimer = 0f;

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(float delta)
    {
        if (updatePathTimer >= updatePathTime) 
        {
            navigationAgent.SetTargetLocation(player.GlobalTransform.origin);
            updatePathTimer = 0;
        } 
        else 
        {
            updatePathTimer += delta;
        }
    }

    Vector3 gravityVec = Vector3.Zero;
    Vector3 snap = Vector3.Zero;

    public override void _PhysicsProcess(float delta)
    {
        HandleGravity(delta);

        if (!navigationAgent.IsTargetReached()) 
        {
            var target = navigationAgent.GetNextLocation();

            if (navigationAgent.IsTargetReachable())
            {
                var position = GlobalTransform.origin;
                var direction = target - GlobalTransform.origin;
                var velocity = MoveAndSlideWithSnap((direction.Normalized() * moveSpeed) + gravityVec, snap, Vector3.Up);
            }
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
