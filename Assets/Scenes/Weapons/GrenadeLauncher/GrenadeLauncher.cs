using Godot;
using System;

public class GrenadeLauncher : WeaponProjectile
{
    float fireRate = 0.75f;
    float fireTimer = 0.0f;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        base._Ready();
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
        //animationPlayer.Play("Stop");
        //animationPlayer.Play("Fire");

        GrenadeLauncherProjectile instance = (GrenadeLauncherProjectile)projectile.Instance();
        instance.Direction = -GlobalTransform.basis.z;
        var transform = instance.GlobalTransform;
        transform.origin = GlobalTransform.origin - (GlobalTransform.basis.z * 3);
        instance.GlobalTransform = transform;

        GetTree().Root.AddChild(instance);
    }

    //  // Called every frame. 'delta' is the elapsed time since the previous frame.
    //  public override void _Process(float delta)
    //  {
    //      
    //  }
}
