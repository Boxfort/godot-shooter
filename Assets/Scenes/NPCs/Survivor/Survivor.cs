using Godot;
using System;
using System.Threading;

public class Survivor : GroundEnemy
{
    [Export]
    PackedScene bullet;
    Vector3 bulletOffset = new Vector3(0, 2.6f, 0);
    ImprovedAudioStreamPlayer3D fireAudio;
    MuzzleFlash muzzleFlash;
    Spatial wanderTarget;

    Random rng = new Random();

    int health;
    bool canMove = true;

    public override int MaxHealth => 25;
    public override int Health { get => health; set => health = value; }
    public override bool CanMove { get => canMove; set => canMove = value; }

    protected override float MoveSpeed => 7.5f;
    protected override float RotationSpeed => 15.0f;
    protected override float Gravity => 40.0f;

    const float attackInaccuracy = 2f;
    const float attackDistance = 30f;
    bool attacking = false;

    // How long each attack lasts
    const float attackTime = 2.0f;
    float attackTimer = 0.0f;


    // How fast to shoot bullets
    const float fireRate = 0.2f;
    float fireTimer = 0.0f;

    // How long between attacks
    const float fireCooldown = 2.0f;
    float fireCooldownTimer = 0f;

    float wanderDistance = 5.0f;
    bool isWandering = false;

    Vector3 lastPosition;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        base._Ready();
        fireAudio = GetNode<ImprovedAudioStreamPlayer3D>("AudioFire");
        muzzleFlash = GetNode<MuzzleFlash>("Armature/MuzzleContainer/MuzzleFlash");
        lastPosition = GlobalTransform.origin;
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(float delta)
    {
        base._Process(delta);
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


        animationTree.Set("parameters/WalkingBlend/blend_amount", CanMove ? Mathf.Abs(currentVelocity.x) + Mathf.Abs(currentVelocity.z) > 0 ? 1 : 0 : 0);
    }

    private void HandleMovement(float delta)
    {
        Vector3 movement = Vector3.Zero;

        if (target != null && !navigationAgent.IsTargetReached() && CanMove && !beingKnockedBack)
        {
            if (GlobalTransform.origin.DistanceTo(target.GlobalTransform.origin) < attackDistance && !attacking && CanSeePlayer())
            {
                lock (this)
                {
                    if (wanderTarget == null)
                    {
                        var dirTo = GlobalTransform.origin.DirectionTo(sightTarget.GlobalTransform.origin);
                        var angleTo = GlobalTransform.basis.z.AngleTo(dirTo);

                        Vector3 randomDirection = new Vector3(((float)rng.NextDouble() * 2) - 1, 0, (float)rng.NextDouble()).Normalized() * wanderDistance;
                        randomDirection = randomDirection.Rotated(Vector3.Up, -angleTo);

                        wanderTarget = new Spatial();
                        GetTree().Root.AddChild(wanderTarget);

                        var transform = wanderTarget.GlobalTransform;
                        transform.origin = GlobalTransform.origin + randomDirection;
                        wanderTarget.GlobalTransform = transform;

                        SetPathfindingTarget(wanderTarget);
                    }
                    else if (GlobalTransform.origin.DistanceTo(lastPosition) < 0.05)
                    {
                        wanderTarget.QueueFree();
                        wanderTarget = null;
                        SetPathfindingTarget(sightTarget);
                    }
                }
                GD.Print(GlobalTransform.origin.DistanceTo(lastPosition));
            }
        }
        else
        {
            SetPathfindingTarget(sightTarget);
        }

        if (target != null && CanReachTarget(target.GlobalTransform.origin, target.GetInstanceId().ToString()))
        {
            movement = GetMovementToTarget();
        }

        var velocity = MoveAndSlideWithSnap(movement, snap, Vector3.Up, maxSlides: 0);
        navigationAgent.SetVelocity(velocity);
    }

    protected override void OnVelocityComputed(Vector3 safeVelocity)
    {
        lastPosition = GlobalTransform.origin;
        base.OnVelocityComputed(safeVelocity);
    }

    private void HandleAttacking(float delta)
    {
        // Should we trigger an attack?
        if (target != null && GlobalTransform.origin.DistanceTo(target.GlobalTransform.origin) < attackDistance && CanSeePlayer() && fireCooldownTimer >= fireCooldown)
        {
            animationTree.Set("parameters/AttackBlend/blend_amount", 1.0f);
            attacking = true;
            CanMove = false;
            fireCooldownTimer = 0;
            lock (this)
            {
                if (wanderTarget != null)
                {
                    wanderTarget.QueueFree();
                    wanderTarget = null;
                }
            }
            SetPathfindingTarget(sightTarget);
        }

        // While we're attacking
        if (attacking && attackTimer < attackTime)
        {
            attackTimer += delta;
            fireTimer += delta;
            LookAtSmooth(target.GlobalTransform.origin, delta);

            if (fireTimer >= fireRate)
            {
                // Fire a bullet!
                fireAudio.Play();
                muzzleFlash.Flash();
                SoldierBullet instance = (SoldierBullet)bullet.Instance();
                instance.Direction = GlobalTransform.origin.DirectionTo(target.GlobalTransform.origin + (Vector3.Down * 2) + GetRandomInaccuracy());
                GetTree().Root.AddChild(instance);
                var transform = instance.GlobalTransform;
                transform.origin = GlobalTransform.origin + bulletOffset;
                instance.GlobalTransform = transform;
                instance.LookAt(target.GlobalTransform.origin, Vector3.Up);
                fireTimer = 0;
            }
        }

        // Should we stop attacking?
        if (attackTimer >= attackTime)
        {
            animationTree.Set("parameters/AttackBlend/blend_amount", 0.0f);
            attackTimer = 0.0f;
            fireTimer = 0.0f;
            attacking = false;
            CanMove = true;
        }

        if (!attacking)
        {
            fireCooldownTimer += delta;
        }
    }

    private Vector3 GetRandomInaccuracy()
    {
        return new Vector3(
            ((float)rng.NextDouble() - 0.5f) * attackInaccuracy,
            ((float)rng.NextDouble() - 0.5f) * attackInaccuracy,
            ((float)rng.NextDouble() - 0.5f) * attackInaccuracy
        );
    }

}
