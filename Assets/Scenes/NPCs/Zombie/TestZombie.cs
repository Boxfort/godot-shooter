using Godot;
using System;
using System.Collections.Generic;

public class TestZombie : GroundAgent, Damageable
{
    AnimationTree animationTree;
    Area attackHitbox;

    float updatePathTime = 0.5f;
    float updatePathTimer = 1.0f;
    float detectionTickTime = 0.5f;
    float detectionTickTimer = 0.5f;

    float moveSpeed = 5;
    float rotationSpeed = 5.0f;
    float gravity = 40.0f;

    bool pausePathfinding = false;

    float currentKnockback = 0.0f;
    Vector3 knockbackDirection = Vector3.Zero;

    bool canMove = true;

    override protected float MoveSpeed => moveSpeed;
    override protected float RotationSpeed => rotationSpeed;
    override protected float Gravity => gravity;

    int health = 25;
    public int Health => health;

    public void TakeDamage(int damage, float knockback, Vector3 fromPosition)
    {
        knockbackDirection = -GlobalTransform.origin.DirectionTo(fromPosition);
        currentKnockback += knockback;
        canMove = false;
    }

    public override void _Ready()
    {
        base._Ready();
        animationTree = GetNode<AnimationTree>("AnimationTree");
        attackHitbox = GetNode<Area>("AttackHitbox");
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
        currentVelocity = Vector3.Zero;
        HandleGravity(delta);
        HandleMovement(delta);
        HandleKnockback(delta);
        HandleAttacking(delta);

        if (target != null && !CanReachTarget(target.GlobalTransform.origin, target.GetInstanceId().ToString()))
        {
            LookAtSmooth(target.GlobalTransform.origin, delta);
        }
        else
        {
            LookAtSmooth(navigationAgent.GetNextLocation(), delta);
        }

        animationTree.Set("parameters/IsRunning/blend_amount", Mathf.Abs(currentVelocity.x) + Mathf.Abs(currentVelocity.z) > 0);
    }

    float attackTime = 0.9f;
    float attackTimer = 0.0f;
    float attackHitTime = 0.55f;
    bool attacking = false;
    bool attackHit = false;

    private void HandleAttacking(float delta)
    {
        if (target != null && GlobalTransform.origin.DistanceTo(target.GlobalTransform.origin) < 3.5f)
        {
            animationTree.Set("parameters/DoAttack/active", true);
            attacking = true;
            pausePathfinding = true;
        }

        if (attacking && attackTimer < attackTime)
        {
            if (attackTimer >= attackHitTime && !attackHit)
            {

                foreach (Area area in attackHitbox.GetOverlappingAreas())
                {
                    if (area is PlayerAreaCollider playerArea)
                    {
                        if (!attackHit)
                        {
                            playerArea.TakeDamage(0, 20.0f, GlobalTransform.origin);
                            attackHit = true;
                        }
                    }
                }
            }

            attackTimer += delta;
        }

        if (attackTimer >= attackTime)
        {
            attackTimer = 0.0f;
            attacking = false;
            attackHit = false;
            pausePathfinding = false;
        }
    }

    private void HandleMovement(float delta)
    {
        Vector3 movement = Vector3.Zero;

        if (target != null && !navigationAgent.IsTargetReached() && !pausePathfinding && canMove)
        {
            if (CanReachTarget(target.GlobalTransform.origin, target.GetInstanceId().ToString()))
            {
                movement = GetMovementToTarget();
            }
        }

        currentVelocity = MoveAndSlideWithSnap(movement + gravityVec + (knockbackDirection * currentKnockback), snap, Vector3.Up);
    }

    private void HandleKnockback(float delta)
    {
        if (currentKnockback > 0)
        {
            currentKnockback -= (Gravity * 4) * delta;

            if (currentKnockback <= 0)
            {
                currentKnockback = 0;
                UpdateTargetPosition();
            }
        }
        else
        {
            canMove = true;
        }
    }
}
