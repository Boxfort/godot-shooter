using System;
using Godot;
using Godot.Collections;

public abstract class WeaponMelee : Weapon
{
    Area hitbox;
    AnimationPlayer animationPlayer;
    bool canFire = true;

    public override bool CanFire => canFire;
    protected abstract int Damage { get; }
    protected abstract float Knockback { get; }

    public override void Equip()
    {
        // TODO
    }

    float attackTime = 0.4f;
    float attackTimer = 0.0f;
    float attackHitStartTime = 0.1f;
    float attackHitEndTime = 0.2f;
    bool attacking = false;
    Dictionary<Damageable, bool> attackHit = new Dictionary<Damageable, bool>() { };

    public override void Fire()
    {
        animationPlayer.Stop(true);
        animationPlayer.Play("Attack");

        attacking = true;
        canFire = false;
    }

    public override void _Ready()
    {
        hitbox = GetNode<Area>("Hitbox");
        animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
    }

    public override void _Process(float delta)
    {
        base._Process(delta);
        HandleAttack(delta);
    }

    private void HandleAttack(float delta)
    {
        if (attacking && attackTimer < attackTime)
        {
            if (attackTimer >= attackHitStartTime && attackTimer < attackHitEndTime)
            {
                foreach (PhysicsBody area in hitbox.GetOverlappingBodies())
                {
                    if (area is Damageable damageable)
                    {
                        bool hasHit = false;
                        attackHit.TryGetValue(damageable, out hasHit);

                        if (!hasHit)
                        {
                            GD.Print("HIT: " + area.Name);
                            damageable.TakeDamage(Damage, Knockback, GlobalTransform.origin);
                            attackHit[damageable] = true;
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
            attackHit.Clear();
            canFire = true;
        }
    }
}