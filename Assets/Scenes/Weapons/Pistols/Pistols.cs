using Godot;
using Godot.Collections;
using System;

public class Pistols : WeaponRaycast
{
    PolyphonicAudioStreamPlayer fireAudio;
    AudioStreamPlayer equipAudio;
    AnimationPlayer animationPlayerLeft;
    AnimationPlayer animationPlayerRight;
    MuzzleFlash muzzleFlashLeft;
    MuzzleFlash muzzleFlashRight;

    bool fireLeft = false;
    bool canFire = true;
    int damage = 5;
    float inaccuracy = 1.0f;
    float range = 100f;
    float fireSpeed = 0.2f;
    float fireTimer = 0;

    public override bool CanFire => canFire;
    protected override float Inaccuracy => inaccuracy;
    protected override float Range => range;
    protected override int Damage => damage;
    protected override float Knockback => 0.0f;

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
        base._Ready();

        Hide();
        fireAudio = GetNode<PolyphonicAudioStreamPlayer>("FireAudio");
        equipAudio = GetNode<AudioStreamPlayer>("EquipAudio");

        animationPlayerLeft = GetNode<AnimationPlayer>("Pistol/AnimationPlayer");
        animationPlayerRight = GetNode<AnimationPlayer>("Pistol2/AnimationPlayer");

        muzzleFlashLeft = GetNode<MuzzleFlash>("Pistol/RootNode/MuzzleFlash");
        muzzleFlashRight = GetNode<MuzzleFlash>("Pistol2/RootNode/MuzzleFlash");

        animationPlayerLeft.Connect("animation_finished", this, nameof(OnAnimationFinished));
        animationPlayerRight.Connect("animation_finished", this, nameof(OnAnimationFinished));
    }

    private void OnAnimationFinished(String animationName)
    {
        //GD.Print("Animation Finished: " + animationName);
    }

    public override void _Process(float delta)
    {
        if (!canFire)
        {
            fireTimer += delta;
            if (fireTimer >= fireSpeed)
            {
                fireTimer = 0;
                canFire = true;
            }
        }
    }
}
