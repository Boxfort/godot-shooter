using System;
using Godot;
using Godot.Collections;

public abstract class WeaponRaycast : Weapon
{
    Random rng = new Random();
    PackedScene debugCollision;

    protected abstract int Damage { get; }
    protected abstract float Knockback { get; }
    protected abstract float Inaccuracy { get; }
    protected abstract float Range { get; }

    public override void _Ready()
    {
        debugCollision = GD.Load("res://Assets/Scenes/Debug/DebugCollision.tscn") as PackedScene;
    }

    protected Dictionary FireRay(bool debugCollisions = false)
    {
        float inaccuracyX = (float)((rng.NextDouble() * (Inaccuracy * 2)) - Inaccuracy);
        float inaccuracyY = (float)((rng.NextDouble() * (Inaccuracy * 2)) - Inaccuracy);

        var origin = GlobalTransform.origin;
        var forward = -GlobalTransform.basis.z.Normalized();
        var left = -GlobalTransform.basis.x.Normalized();
        var up = -GlobalTransform.basis.y.Normalized();

        var target = origin + (forward * Range);
        target += left * inaccuracyX;
        target += up * inaccuracyY;

        var directState = GetWorld().DirectSpaceState;
        var collision = directState.IntersectRay(origin, target);

        if (debugCollisions)
        {
            if (collision.Contains("position"))
            {
                var instance = debugCollision.Instance() as Spatial;
                GetTree().Root.AddChild(instance);
                var gt = instance.GlobalTransform;
                gt.origin = (Vector3)collision["position"];
                instance.GlobalTransform = gt;
            }

            if (collision.Contains("collider"))
            {
                Node collider = (Node)collision["collider"];

                if (collider is Damageable damageable)
                {
                    damageable.TakeDamage(Damage, Knockback, GlobalTransform.origin);
                }
            }
        }

        return collision;
    }
}