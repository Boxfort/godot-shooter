using Godot;
using System;

public class GrenadeLauncher : WeaponProjectile
{
    RayCast raycast;
    AudioStreamPlayer shootAudio;

    float shotDistance = 3.0f;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        base._Ready();
        raycast = GetNode<RayCast>("RayCast");
        shootAudio = GetNode<AudioStreamPlayer>("AudioStreamPlayer");
    }

    public override void Fire()
    {
        canFire = false;
        animationPlayer.Stop();
        animationPlayer.Play("Fire");
        shootAudio.Play();

        GrenadeLauncherProjectile instance = (GrenadeLauncherProjectile)projectile.Instance();
        GetTree().Root.AddChild(instance);

        instance.Direction = -GlobalTransform.basis.z;
        var transform = instance.GlobalTransform;
        transform.origin = GlobalTransform.origin - (GlobalTransform.basis.z * shotDistance);
        transform.basis = GlobalTransform.basis;
        instance.GlobalTransform = transform;

        if (raycast.IsColliding())
        {
            transform.origin = raycast.GetCollisionPoint();
            instance.GlobalTransform = transform;

            instance.Explode();
        }
    }
}
