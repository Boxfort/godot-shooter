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
    protected abstract float AttackTime { get; }
    protected abstract float AttackHitStartTime { get; }
    protected abstract float AttackHitEndTime { get; }
    protected bool attacking = false;
    protected float attackTimer = 0.0f;
    protected Dictionary<Damageable, bool> attackHit = new Dictionary<Damageable, bool>() { };

    public override void Equip()
    {
        Show();
        animationPlayer.Play("Equip");
    }

    protected virtual void OnAttackHit()
    {
        // no-op
    }

    public override void Fire()
    {
        Show();
        animationPlayer.Stop(true);
        animationPlayer.Play("Attack");

        attacking = true;
        canFire = false;
    }

    public override void _Ready()
    {
        Hide();
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
        if (attacking && attackTimer < AttackTime)
        {
            if (attackTimer >= AttackHitStartTime && attackTimer < AttackHitEndTime)
            {
                foreach (Node area in hitbox.GetOverlappingBodies())
                {
                    if (area is Damageable damageable)
                    {
                        bool hasHit = false;
                        attackHit.TryGetValue(damageable, out hasHit);

                        if (!hasHit)
                        {
                            damageable.TakeDamage(Damage, Knockback, GlobalTransform.origin);
                            attackHit[damageable] = true;
                            OnAttackHit();
                        }
                    }
                }
            }

            attackTimer += delta;
        }

        if (attackTimer >= AttackTime)
        {
            attackTimer = 0.0f;
            attacking = false;
            attackHit.Clear();
            canFire = true;
        }
    }
}