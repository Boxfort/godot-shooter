using Godot;
using Godot.Collections;
using System;

public class Pistols : WeaponRaycast
{
    ImprovedAudioStreamPlayer fireAudio;
    AudioStreamPlayer equipAudio;
    AnimationPlayer animationPlayerLeft;
    AnimationPlayer animationPlayerRight;
    MuzzleFlash muzzleFlashLeft;
    MuzzleFlash muzzleFlashRight;

    bool fireLeft = false;
    int damage = 5;
    float inaccuracy = 1.0f;
    float range = 100f;

    public override bool CanFire => canFire;
    protected override float Inaccuracy => inaccuracy;
    protected override float Range => range;
    protected override int Damage => damage;
    protected override float Knockback => 0.0f;
    public override float FireSpeed => 0.2f;

    public override void Equip()
    {
        equipAudio.Play();
        animationPlayerLeft.Play("Equip");
        animationPlayerRight.Play("Equip");
        Show();
    }

    public override void Fire()
    {
        canFire = false;

        fireAudio.Play();

        if (fireLeft)
        {
            animationPlayerLeft.Stop(true);
            animationPlayerLeft.Play("Fire");
            fireLeft = false;
            muzzleFlashLeft.Flash();
        }
        else
        {
            animationPlayerRight.Stop(true);
            animationPlayerRight.Play("Fire");
            fireLeft = true;
            muzzleFlashRight.Flash();
        }

        Dictionary collision = FireRay(true);
    }

    public override void _Ready()
    {
        customAnimationPlayer = true;
        base._Ready();

        Hide();
        fireAudio = GetNode<ImprovedAudioStreamPlayer>("FireAudio");
        equipAudio = GetNode<AudioStreamPlayer>("EquipAudio");

        animationPlayerLeft = GetNode<AnimationPlayer>("Pistol/AnimationPlayer");
        animationPlayerRight = GetNode<AnimationPlayer>("Pistol2/AnimationPlayer");

        muzzleFlashLeft = GetNode<MuzzleFlash>("Pistol/RootNode/FlashContainer/MuzzleFlash");
        muzzleFlashRight = GetNode<MuzzleFlash>("Pistol2/RootNode/FlashContainer/MuzzleFlash");

        animationPlayerLeft.Connect("animation_finished", this, nameof(OnAnimationFinished));
    }
}
