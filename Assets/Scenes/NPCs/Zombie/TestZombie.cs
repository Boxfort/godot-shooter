using Godot;
using System;
using System.Collections.Generic;

public class TestZombie : GroundAgent, Damageable
{
    AnimationTree animationTree;

    float updatePathTime = 0.5f;
    float updatePathTimer = 1.0f;
    float detectionTickTime = 0.5f;
    float detectionTickTimer = 0.5f;

    float moveSpeed = 5;
    float rotationSpeed = 5.0f;

    override protected float MoveSpeed { get => moveSpeed; set => moveSpeed = value; }
    override protected float RotationSpeed { get => rotationSpeed; set => rotationSpeed = value; }

    int health = 25;
    public int Health => health;

    public void TakeDamage(int damage)
    {
        GD.Print("Taking damage: " + damage);
    }

    public override void _Ready()
    {
        base._Ready();
        animationTree = GetNode<AnimationTree>("AnimationTree");
    }

    protected override void OnBodyEnteredDectection(PhysicsBody body)
    {
        if (body is CharacterController)
        {
            SetTarget(body as Spatial);
        }
    }

    protected override void OnBodyExitedDectection(PhysicsBody body)
    {
        if (body == target)
        {
            SetTarget(target = null);
        }
    }

    public override void _Process(float delta)
    {
        // TODO: move this to parent class?
        if (target != null)
        {
            if (updatePathTimer >= updatePathTime)
            {
                UpdateTargetPosition();
                updatePathTimer = 0;
            }
            else
            {
                updatePathTimer += delta;
            }
        }
    }

    public override void _PhysicsProcess(float delta)
    {
        base._PhysicsProcess(delta);

        if (target != null && !CanReachTarget(target.GlobalTransform.origin, target.GetInstanceId().ToString()))
        {
            LookAtSmooth(target.GlobalTransform.origin, delta);
        }

        animationTree.Set("parameters/IsRunning/blend_amount", Mathf.Abs(currentVelocity.x) + Mathf.Abs(currentVelocity.z) > 0);
    }
}
