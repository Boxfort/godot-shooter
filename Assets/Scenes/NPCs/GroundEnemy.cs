using Godot;
using System;

public abstract class GroundEnemy : GroundAgent, Damageable
{
    [Export]
    PackedScene gibs;

    protected AnimationTree animationTree;
    AudioStreamPlayer3D deathAudio;
    ImprovedAudioStreamPlayer3D alertAudio;
    ImprovedAudioStreamPlayer3D hitAudio;

    public abstract int MaxHealth { get; }
    public abstract int Health { get; set; }
    public abstract bool CanMove { get; set; }

    float updatePathTime = 0.5f;
    float updatePathTimer = 1.0f;

    bool playerInRadius = false;
    protected Spatial sightTarget;

    float currentKnockback = 0.0f;
    Vector3 knockbackDirection = Vector3.Zero;
    protected bool beingKnockedBack = true;

    protected bool isDead = false;
    const float accumulatedDamageTime = 0.05f;
    float accumulatedDamageTimer = 0;
    int accumulatedDamage = 0;

    bool deathHandled = false;
    const float deathVelocityDecay = 15;
    const float initialDeathVelocity = 15;
    float deathVelocity = 0;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        base._Ready();

        animationTree = GetNode<AnimationTree>("AnimationTree");
        hitAudio = GetNode<ImprovedAudioStreamPlayer3D>("AudioHit");
        deathAudio = GetNode<AudioStreamPlayer3D>("AudioDeath");
        alertAudio = GetNode<ImprovedAudioStreamPlayer3D>("AudioAlert");

        Health = MaxHealth;

        navigationAgent.Connect("velocity_computed", this, nameof(OnVelocityComputed));
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

        // Check to see if the player is visible.
        if (playerInRadius && target == null && CanSeePlayer())
        {
            GD.Print(Name + " spotted player");

            if (alertAudio != null)
                alertAudio.Play();
            SetPathfindingTarget(sightTarget);
        }
    }

    protected bool CanSeePlayer()
    {
        var directState = GetWorld().DirectSpaceState;
        var collisionBody = directState.IntersectRay(GlobalTransform.origin + (Vector3.Up / 2), sightTarget.GlobalTransform.origin, collisionMask: 3);
        var collisionHead = directState.IntersectRay(GlobalTransform.origin + (Vector3.Up), sightTarget.GlobalTransform.origin + Vector3.Up, collisionMask: 3);

        if (collisionBody.Contains("collider"))
        {
            Node collider = (Node)collisionBody["collider"];

            if (collider is CharacterController)
            {
                return true;
            }
        }

        if (collisionHead.Contains("collider"))
        {
            Node collider = (Node)collisionHead["collider"];

            if (collider is CharacterController)
            {
                return true;
            }
        }

        return false;
    }

    protected override void OnBodyEnteredDectection(PhysicsBody body)
    {
        if (body is CharacterController player && !isDead)
        {
            playerInRadius = true;
            sightTarget = player;
        }
    }

    protected override void OnBodyExitedDectection(PhysicsBody body)
    {
        if (body == target && !isDead)
        {
            playerInRadius = false;
            SetPathfindingTarget(null);
        }
    }

    protected virtual void OnVelocityComputed(Vector3 safeVelocity)
    {
        if (CanMove)
        {
            currentVelocity = MoveAndSlideWithSnap(safeVelocity + gravityVec + (knockbackDirection * currentKnockback), snap, Vector3.Up);
        }
    }

    public void TakeDamage(int damage, float knockback, Vector3 fromPosition)
    {
        if (deathHandled)
            return;

        if (!isDead)
        {
            knockbackDirection = -GlobalTransform.origin.DirectionTo(fromPosition);
            currentKnockback += knockback;

            beingKnockedBack = true;
            animationTree.Set("parameters/HitReaction/active", true);
        }

        Health -= damage;

        if (accumulatedDamageTimer < accumulatedDamageTime)
        {
            accumulatedDamage += damage;
        }
        else
        {
            accumulatedDamage = damage;
            accumulatedDamageTimer = 0;
        }

        if (Health <= 0)
        {
            isDead = true;
        }
        else
        {
            hitAudio.Play();
        }
    }

    protected void HandleAccumulatedDamageCounter(float delta)
    {
        if (accumulatedDamageTimer < accumulatedDamageTime)
        {
            accumulatedDamageTimer += delta;
        }
    }

    protected void HandleDead(float delta)
    {
        HandleGravity(delta);

        if (!deathHandled)
        {
            // If we do lots of damage to kill then gib
            if (accumulatedDamage >= (MaxHealth / 2))
            {
                Spatial instance = (Spatial)gibs.Instance();
                GetTree().Root.AddChild(instance);
                instance.GlobalTransform = GlobalTransform;
                QueueFree();
            }

            deathHandled = true;

            // Only collide with the level.
            CollisionLayer = 8;
            CollisionMask = 1;

            animationTree.Set("parameters/DeathBlend/blend_amount", 1);
            deathAudio.Play();

            // Death Knockback
            gravityVec = Vector3.Up * initialDeathVelocity;
            snap = Vector3.Zero;
            deathVelocity = initialDeathVelocity;
            navigationAgent.QueueFree();
        }

        currentVelocity = MoveAndSlideWithSnap((knockbackDirection * deathVelocity) + gravityVec, snap, Vector3.Up);


        if (deathVelocity > 0)
        {
            deathVelocity -= delta * deathVelocityDecay;
        }
        else
        {
            deathVelocity = 0;
        }
    }

    protected void HandleKnockback(float delta)
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