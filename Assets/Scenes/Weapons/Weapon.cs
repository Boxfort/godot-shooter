using System;
using Godot;
using Godot.Collections;

public abstract class Weapon: Spatial
{
    Random rng = new Random();
    PackedScene debugCollision;

    public abstract bool CanFire { get; }

    public abstract void Fire();

    public abstract void Equip();

    public override void _Ready()
    {
        debugCollision = GD.Load("res://Assets/Scenes/Debug/DebugCollision.tscn") as PackedScene;
    }

    protected Dictionary FireRay(float inaccuracy, float distance, bool debugCollisions = false) 
    {
        float inaccuracyX = (float)((rng.NextDouble() * (inaccuracy * 2)) - inaccuracy);
        float inaccuracyY = (float)((rng.NextDouble() * (inaccuracy * 2)) - inaccuracy);

        var origin = GlobalTransform.origin;
        var forward = -GlobalTransform.basis.z.Normalized();
        var left = -GlobalTransform.basis.x.Normalized();
        var up = -GlobalTransform.basis.y.Normalized();

        var target = origin + (forward * distance);
        target += left * inaccuracyX;
        target += up * inaccuracyY;

        var directState = GetWorld().DirectSpaceState;
		var collision =  directState.IntersectRay(origin, target);

        if (debugCollisions) 
        {
            if(collision.Contains("position"))
            {
                GD.Print(collision["position"]);
                var instance = debugCollision.Instance() as Spatial;
                GetTree().Root.AddChild(instance);
                var gt = instance.GlobalTransform;
                gt.origin = (Vector3)collision["position"];
                instance.GlobalTransform = gt;
            }
        }

        return collision;
    }
}