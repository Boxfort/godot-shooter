using Godot;
using System;

public class ZombieDogCollider : CollisionShape, Damageable
{
    [Signal]
    delegate void OnTakenDamage(int health, float knockback, Vector3 fromPosition);

    public void TakeDamage(int damage, float knockback, Vector3 fromPosition)
    {
        EmitSignal(nameof(OnTakenDamage), damage, knockback, fromPosition);
    }
}
