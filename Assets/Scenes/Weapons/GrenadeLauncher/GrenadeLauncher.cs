using Godot;
using System;

public class GrenadeLauncher : WeaponProjectile
{
    RayCast raycast;
    AudioStreamPlayer shootAudio;

    float fireRate = 0.75f;
    float fireTimer = 0.0f;
    float shotDistance = 3.0f;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        base._Ready();
        canFire = true;
        raycast = GetNode<RayCast>("RayCast");
        shootAudio = GetNode<AudioStreamPlayer>("AudioStreamPlayer");
    }

    public override void _Process(float delta)
    {
        if (!canFire)
        {
            if (fireTimer >= fireRate)
            {
                canFire = true;
                fireTimer = 0;
            }
            else
            {
                fireTimer += delta;
            }
        }
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

    //  // Called every frame. 'delta' is the elapsed time since the previous frame.
    //  public override void _Process(float delta)
    //  {
    //      
    //  }
}
