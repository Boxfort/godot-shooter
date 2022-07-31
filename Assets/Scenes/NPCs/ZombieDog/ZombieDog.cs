using Godot;
using System;

public class ZombieDog : GroundEnemy
{
    AudioStreamPlayer3D attackAudio;

    protected override float MoveSpeed => 10.0f;
    protected override float RotationSpeed => 7.5f;
    protected override float Gravity => 40.0f;

    int health = 0;
    bool canMove = true;

    public override int Health { get => health; set => health = value; }
    public override bool CanMove { get => canMove; set => canMove = value; }
    public override int MaxHealth => 15;

    const float attackDamageCooldown = 1.0f;
    float attackDamageCooldownTimer = 1.0f;
    const int attackDamage = 10;
    const int attackKnockback = 10;
    const float attackDistance = 9.0f;
    const float initialLeapVelocity = 20;
    const float attackCooldown = 2.0f;
    float attackCooldownTimer = 2.0f;

    float leapVelocity = 0;
    bool attacking = false;
    bool hasLeaped = false;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        base._Ready();
        attackAudio = GetNode<AudioStreamPlayer3D>("AudioAttack");
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
        HandleAttacking(delta);
        HandleDamage(delta);
        HandleKnockback(delta);

        // TODO: something better than this shitty guard statement
        if (attacking)
            return;

        HandleMovement(delta);

        if (target != null && !CanReachTarget(target.GlobalTransform.origin, target.GetInstanceId().ToString()))
        {
            LookAtSmooth(target.GlobalTransform.origin, delta);
        }
        else
        {
            LookAtSmooth(navigationAgent.GetNextLocation(), delta);
        }

        animationTree.Set("parameters/RunningBlend/blend_amount", Mathf.Abs(currentVelocity.x) + Mathf.Abs(currentVelocity.z) > 0);
    }

    private void HandleDamage(float delta)
    {
        if (attackDamageCooldownTimer >= attackDamageCooldown)
        {
            int slides = GetSlideCount();

            for (int i = 0; i < slides; i++)
            {
                KinematicCollision collision = GetSlideCollision(i);

                Node node = (Node)collision.Collider;

                if (node == null)
                    return;

                if (node.IsInGroup("player") || node is Damageable)
                {
                    ((Damageable)node).TakeDamage(attackDamage, attackKnockback, GlobalTransform.origin);
                    attackDamageCooldownTimer = 0f;
                }
            }
        }
        else
        {
            attackDamageCooldownTimer += delta;
        }
    }

    private void HandleMovement(float delta)
    {
        Vector3 movement = Vector3.Zero;

        if (target != null && !navigationAgent.IsTargetReached() && CanMove)
        {
            if (CanReachTarget(target.GlobalTransform.origin, target.GetInstanceId().ToString()))
            {
                movement = GetMovementToTarget();
            }
        }

        var velocity = MoveAndSlideWithSnap(movement, snap, Vector3.Up, maxSlides: 0);
        navigationAgent.SetVelocity(velocity);
    }

    private void HandleAttacking(float delta)
    {
        if (attacking)
        {
            if (!hasLeaped)
            {
                hasLeaped = true;

                attackCooldownTimer = 0;
                LookAt(target.GlobalTransform.origin, delta);
                gravityVec = Vector3.Up * 12;
                snap = Vector3.Zero;
                leapVelocity = initialLeapVelocity;
                attackAudio.Play();
            }
            else
            {
                if (IsOnFloor())
                {
                    attacking = false;
                    hasLeaped = false;
                    CanMove = true;
                }
            }

            currentVelocity = MoveAndSlideWithSnap((GlobalTransform.basis.z * leapVelocity) + gravityVec, snap, Vector3.Up);

            if (leapVelocity > 0)
            {
                leapVelocity -= delta * 15;
            }
            else
            {
                leapVelocity = 0;
            }
        }
        else
        {
            // Should we trigger an attack?
            if (target != null && GlobalTransform.origin.DistanceTo(target.GlobalTransform.origin) <= attackDistance && attackCooldownTimer >= attackCooldown)
            {
                animationTree.Set("parameters/AttackingBlend/active", 1);
                attacking = true;
                CanMove = false;
            }

            if (attackCooldownTimer < attackCooldown)
            {
                attackCooldownTimer += delta;
            }
        }
    }
}