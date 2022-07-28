using Godot;
using System;
using System.Collections.Generic;

public class TestZombie : GroundAgent, Damageable
{
    AnimationTree animationTree;
    Area attackHitbox;

    ImprovedAudioStreamPlayer3D moanAudio;
    ImprovedAudioStreamPlayer3D hitAudio;
    AudioStreamPlayer3D deathAudio;

    float updatePathTime = 0.5f;
    float updatePathTimer = 1.0f;

    float moveSpeed = 7.5f;
    float rotationSpeed = 5.0f;
    float gravity = 40.0f;

    bool pausePathfinding = false;

    float currentKnockback = 0.0f;
    Vector3 knockbackDirection = Vector3.Zero;

    bool beingKnockedBack = true;

    override protected float MoveSpeed => moveSpeed;
    override protected float RotationSpeed => rotationSpeed;
    override protected float Gravity => gravity;

    int health = 25;
    public int Health => health;

    const float attackDistance = 3.5f;
    const float attackTime = 0.9f;
    float attackTimer = 0.0f;
    const float attackHitTime = 0.55f;
    bool attacking = false;
    bool attackHit = false;

    bool isDead = false;
    bool deathHandled = false;
    float deathVelocity = 0;

    public void TakeDamage(int damage, float knockback, Vector3 fromPosition)
    {
        if (isDead)
            return;

        knockbackDirection = -GlobalTransform.origin.DirectionTo(fromPosition);
        currentKnockback += knockback;

        beingKnockedBack = true;

        health -= damage;
        animationTree.Set("parameters/HitReaction/active", true);

        if (health <= 0)
        {
            isDead = true;
        }
        else
        {
            hitAudio.Play();
        }
    }

    public override void _Ready()
    {
        base._Ready();
        animationTree = GetNode<AnimationTree>("AnimationTree");
        attackHitbox = GetNode<Area>("AttackHitbox");
        moanAudio = GetNode<ImprovedAudioStreamPlayer3D>("AudioMoan");
        hitAudio = GetNode<ImprovedAudioStreamPlayer3D>("AudioHit");
        deathAudio = GetNode<AudioStreamPlayer3D>("AudioDeath");
        navigationAgent = GetNode<NavigationAgent>("NavigationAgent");

        navigationAgent.Connect("velocity_computed", this, nameof(OnVelocityComputed));
    }

    protected override void OnBodyEnteredDectection(PhysicsBody body)
    {
        if (body is CharacterController)
        {
            SetTarget(body as Spatial);
            moanAudio.Play();
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
        if (isDead)
        {
            HandleDead(delta);
            return;
        }

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


    private void HandleDead(float delta)
    {
        HandleGravity(delta);

        if (!deathHandled)
        {
            deathHandled = true;

            // Only collide with the level.
            CollisionLayer = 8;
            CollisionMask = 1;

            animationTree.Set("parameters/IsDead/blend_amount", 1);
            deathAudio.Play();

            // Death Knockback
            gravityVec = Vector3.Up * 15;
            snap = Vector3.Zero;
            deathVelocity = 15;
        }

        currentVelocity = MoveAndSlideWithSnap((knockbackDirection * deathVelocity) + gravityVec, snap, Vector3.Up);


        if (deathVelocity > 0)
        {
            deathVelocity -= delta * 15;
        }
        else
        {
            deathVelocity = 0;
        }
    }

    private void HandleAttacking(float delta)
    {
        if (target != null && GlobalTransform.origin.DistanceTo(target.GlobalTransform.origin) < attackDistance)
        {
            animationTree.Set("parameters/DoAttack/active", true);
            attacking = true;
            pausePathfinding = true;
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
            pausePathfinding = false;
        }
    }

    private void HandleMovement(float delta)
    {
        Vector3 movement = Vector3.Zero;

        if (target != null && !navigationAgent.IsTargetReached() && !pausePathfinding && !beingKnockedBack)
        {
            if (CanReachTarget(target.GlobalTransform.origin, target.GetInstanceId().ToString()))
            {
                movement = GetMovementToTarget();
            }
        }

        var velocity = MoveAndSlideWithSnap(movement, snap, Vector3.Up, maxSlides: 0);
        navigationAgent.SetVelocity(velocity);
    }

    void OnVelocityComputed(Vector3 safeVelocity)
    {
        if (!pausePathfinding)
        {
            currentVelocity = MoveAndSlideWithSnap(safeVelocity + gravityVec /*+ (knockbackDirection * currentKnockback)*/, snap, Vector3.Up);
        }
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
            beingKnockedBack = false;
        }
    }
}
