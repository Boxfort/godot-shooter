using Godot;
using System;

public class VentCover : StaticBody, Damageable
{
    public void TakeDamage(int damage, float knockback, Vector3 fromPosition)
    {
        QueueFree();
    }
}
