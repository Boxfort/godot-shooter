using Godot;
using System;
using System.Collections.Generic;

public class TestZombie : GroundEnemy
{
    Area attackHitbox;

    ImprovedAudioStreamPlayer3D moanAudio;
    ImprovedAudioStreamPlayer3D hitAudio;

    override protected float MoveSpeed => 7.5f;
    override protected float RotationSpeed => 5.0f;
    override protected float Gravity => 40.0f;

    int health = 25;
    bool canMove = true;

    public override int MaxHealth => 25;
    public override int Health { get => health; set => health = value; }
    public override bool CanMove { get => canMove; set => canMove = value; }

    const float attackDistance = 3.5f;
    const float attackTime = 0.9f;
    float attackTimer = 0.0f;
    const float attackHitTime = 0.55f;
    bool attacking = false;
    bool attackHit = false;

    public override void _Ready()
    {
        base._Ready();
        attackHitbox = GetNode<Area>("AttackHitbox");
        moanAudio = GetNode<ImprovedAudioStreamPlayer3D>("AudioMoan");
    }

    public override void _PhysicsProcess(float delta)
    {
        if (isDead)
        {
            HandleDead(delta);
            return;
        }

        HandleAccumulatedDamageCounter(delta);
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

    private void HandleAttacking(float delta)
    {
        // Should we trigger an attack?
        if (target != null && GlobalTransform.origin.DistanceTo(target.GlobalTransform.origin) < attackDistance)
        {
            animationTree.Set("parameters/DoAttack/active", true);
            attacking = true;
            CanMove = false;
        }

        if (attacking && attackTimer < attackTime)
        {
            if (attackTimer >= attackHitTime && !attackHit)
            {
                foreach (PhysicsBody body in attackHitbox.GetOverlappingBodies())
                {
                    GD.Print(body.Name);

                    if (body is Damageable damageable)
                    {
                        if (!attackHit)
                        {
                            damageable.TakeDamage(10, 20.0f, GlobalTransform.origin);
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
            CanMove = true;
        }
    }

    private void HandleMovement(float delta)
    {
        Vector3 movement = Vector3.Zero;

        if (target != null && !navigationAgent.IsTargetReached() && CanMove && !beingKnockedBack)
        {
            if (CanReachTarget(target.GlobalTransform.origin, target.GetInstanceId().ToString()))
            {
                movement = GetMovementToTarget();
            }
        }

        var velocity = MoveAndSlideWithSnap(movement, snap, Vector3.Up, maxSlides: 0);
        navigationAgent.SetVelocity(velocity);
    }

}
