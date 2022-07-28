using Godot;
using Godot.Collections;
using System;

public class AssaultRifle : WeaponRaycast
{
    public override bool CanFire => canFire;

    protected override int Damage => 5;

    protected override float Knockback => 1.0f;

    protected override float Inaccuracy => 1.25f;

    protected override float Range => 100f;

    public override float FireSpeed => 0.15f;

    ImprovedAudioStreamPlayer fireAudio;
    AudioStreamPlayer equipAudio;
    MuzzleFlash muzzleFlash;

    public override void Equip()
    {
        Show();
        animationPlayer.Stop(true);
        animationPlayer.Play("Equip");
        equipAudio.Play();
    }

    public override void Fire()
    {
        canFire = false;

        fireAudio.Play();

        animationPlayer.Stop(true);
        animationPlayer.Play("Fire");
        muzzleFlash.Flash();

        Dictionary collision = FireRay(true);
    }

    public override void _Ready()
    {
        base._Ready();

        Hide();

        fireAudio = GetNode<ImprovedAudioStreamPlayer>("FireAudio");
        equipAudio = GetNode<AudioStreamPlayer>("EquipAudio");
        muzzleFlash = GetNode<MuzzleFlash>("Model/Obokan/FlashContainer/MuzzleFlash");
    }
}
