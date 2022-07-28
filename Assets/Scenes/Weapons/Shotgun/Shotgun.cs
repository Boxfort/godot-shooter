using Godot;
using Godot.Collections;
using System;

public class Shotgun : WeaponRaycast
{
    AudioStreamPlayer fireAudio;
    AudioStreamPlayer equipAudio;
    MuzzleFlash muzzleFlash;

    int damage = 2;
    float inaccuracy = 2.0f;
    float range = 60.0f;
    int pellets = 8;

    public override float FireSpeed => 1f;
    public override bool CanFire => canFire;

    protected override float Inaccuracy => inaccuracy;
    protected override float Range => range;
    protected override int Damage => damage;
    protected override float Knockback => 3f;

    public override void Equip()
    {
        animationPlayer.Play("Equip");
        equipAudio.Play();
        Show();
    }

    public override void Fire()
    {
        canFire = false;
        animationPlayer.Stop(true);
        animationPlayer.Play("Fire");
        fireAudio.Play();
        muzzleFlash.Flash();

        for (int i = 0; i < pellets; i++)
        {
            Dictionary collision = FireRay(true);
        }
    }

    public override void _Ready()
    {
        base._Ready();

        animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
        fireAudio = GetNode<AudioStreamPlayer>("AudioFire");
        equipAudio = GetNode<AudioStreamPlayer>("AudioEquip");
        muzzleFlash = GetNode<MuzzleFlash>("Spas/MuzzleFlashHolder/MuzzleFlash");

        Hide();
    }

}
