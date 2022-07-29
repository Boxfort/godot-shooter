using Godot;
using System;

public class ZombieDog : GroundAgent, Damageable
{
    AnimationTree animationTree;
    ImprovedAudioStreamPlayer3D hitAudio;
    AudioStreamPlayer3D attackAudio;
    AudioStreamPlayer3D deathAudio;

    [Export]
    PackedScene gibs;

    protected override float MoveSpeed => 10.0f;

    protected override float RotationSpeed => 7.5f;

    protected override float Gravity => 40.0f;

    float updatePathTime = 0.5f;
    float updatePathTimer = 1.0f;
    bool pausePathfinding = false;

    const int maxHealth = 15;
    int health = maxHealth;
    public int Health => health;

    float currentKnockback = 0.0f;
    Vector3 knockbackDirection = Vector3.Zero;

    bool beingKnockedBack = true;

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

    bool isDead = false;

    float accumulatedDamageTimer = 0;
    const float accumulatedDamageTime = 0.05f;
    int accumulatedDamage = 0;

    public void TakeDamage(int damage, float knockback, Vector3 fromPosition)
    {
        if (isDead)
            return;

        knockbackDirection = -GlobalTransform.origin.DirectionTo(fromPosition);
        currentKnockback += knockback;

        beingKnockedBack = true;
        animationTree.Set("parameters/HitReaction/active", true);

        health -= damage;

        if (accumulatedDamageTimer < accumulatedDamageTime)
        {
            accumulatedDamage += damage;
        }
        else
        {
            accumulatedDamage = damage;
            accumulatedDamageTimer = 0;
        }

        if (health <= 0)
        {
            isDead = true;
        }
        else
        {
            hitAudio.Play();
        }
    }

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        base._Ready();
        animationTree = GetNode<AnimationTree>("AnimationTree");
        hitAudio = GetNode<ImprovedAudioStreamPlayer3D>("AudioHit");
        attackAudio = GetNode<AudioStreamPlayer3D>("AudioAttack");
        deathAudio = GetNode<AudioStreamPlayer3D>("AudioDeath");

        navigationAgent.Connect("velocity_computed", this, nameof(OnVelocityComputed));
    }

    protected override void OnBodyEnteredDectection(PhysicsBody body)
    {
        if (body is CharacterController && !isDead)
        {
            SetTarget(body as Spatial);
        }
    }

    protected override void OnBodyExitedDectection(PhysicsBody body)
    {
        if (body == target && !isDead)
        {
            SetTarget(target = null);
        }
    }

    public override void _Process(float delta)
    {
        if (target != null && !isDead)
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

    private void HandleAccumulatedDamageCounter(float delta)
    {
        if (accumulatedDamageTimer < accumulatedDamageTime)
        {
            accumulatedDamageTimer += delta;
        }
    }

    bool deathHandled = false;
    float deathVelocity = 0;

    private void HandleDead(float delta)
    {
        HandleGravity(delta);

        if (!deathHandled)
        {
            if (accumulatedDamage >= (maxHealth / 2))
            {
                // GIB
                Spatial instance = (Spatial)gibs.Instance();
                GetTree().Root.AddChild(instance);
                instance.GlobalTransform = GlobalTransform;
                QueueFree();
            }

            deathHandled = true;

            // Only collide with the level.
            CollisionLayer = 8;
            CollisionMask = 1;

            animationTree.Set("parameters/DeadBlend/blend_amount", 1);
            deathAudio.Play();

            // Death Knockback
            gravityVec = Vector3.Up * 15;
            snap = Vector3.Zero;
            deathVelocity = 15;
            navigationAgent.QueueFree();
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

        if (target != null && !navigationAgent.IsTargetReached())// && !pausePathfinding && !beingKnockedBack)
        {
            if (CanReachTarget(target.GlobalTransform.origin, target.GetInstanceId().ToString()))
            {
                movement = GetMovementToTarget();
            }
        }

        //currentVelocity = MoveAndSlideWithSnap(movement + gravityVec, snap, Vector3.Up); // + (knockbackDirection * currentKnockback), snap, Vector3.Up);
        var velocity = MoveAndSlideWithSnap(movement, snap, Vector3.Up, maxSlides: 0);
        navigationAgent.SetVelocity(velocity);
    }

    void OnVelocityComputed(Vector3 safeVelocity)
    {
        if (!attacking)
        {
            currentVelocity = MoveAndSlideWithSnap(safeVelocity + gravityVec + (knockbackDirection * currentKnockback), snap, Vector3.Up);
        }
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
                pausePathfinding = true;
            }

            if (attackCooldownTimer < attackCooldown)
            {
                attackCooldownTimer += delta;
            }
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
