using Godot;
using System;

public class ZombieDog : GroundAgent, Damageable
{
    AnimationTree animationTree;

    protected override float MoveSpeed => 10.0f;

    protected override float RotationSpeed => 7.5f;

    protected override float Gravity => 40.0f;

    float updatePathTime = 0.5f;
    float updatePathTimer = 1.0f;
    bool pausePathfinding = false;

    public void TakeDamage(int damage, float knockback, Vector3 fromPosition)
    {
        GD.Print("Dog hit");
    }

    // Called when the node enters the scene tree for the first time.
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
        HandleGravity(delta);
        HandleAttacking(delta);
        HandleDamage(delta);

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

    const float attackDamageCooldown = 1.0f;
    float attackDamageCooldownTimer = 1.0f;
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
                    ((Damageable)node).TakeDamage(15, 10, GlobalTransform.origin);
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

        currentVelocity = MoveAndSlideWithSnap(movement + gravityVec, snap, Vector3.Up); // + (knockbackDirection * currentKnockback), snap, Vector3.Up);
    }

    const float attackDistance = 9.0f;
    const float initialLeapVelocity = 20;
    const float attackCooldown = 2.0f;
    float attackCooldownTimer = 2.0f;

    float leapVelocity = 0;
    bool attacking = false;
    bool hasLeaped = false;

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
}
