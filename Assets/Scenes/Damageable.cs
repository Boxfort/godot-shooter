using Godot;
using System;

public interface Damageable
{
    void TakeDamage(int damage, float knockback, Vector3 fromPosition);
}
