using Godot;
using Godot.Collections;
using System;

public class Shotgun : WeaponRaycast
{
    AnimationPlayer animationPlayer;

    bool canFire = true;
    int damage = 2;
    float inaccuracy = 2.0f;
    float range = 60.0f;
    int pellets = 8;
    float fireSpeed = 1f;
    float fireTimer = 0;

    public override bool CanFire => canFire;

    protected override float Inaccuracy => inaccuracy;
    protected override float Range => range;

    protected override int Damage => damage;

    public override void Equip()
    {
        animationPlayer.Play("Equip");
        Show();
    }

    public override void Fire()
    {
        canFire = false;
        animationPlayer.Stop(true);
        animationPlayer.Play("Fire");

        for(int i = 0; i < pellets; i++)
        {
            Dictionary collision = FireRay(true);
        }
    }

    public override void _Ready()
    {
        base._Ready();

        animationPlayer = GetNode<AnimationPlayer>("Model/AnimationPlayer");
        Hide();
    }

    public override void _Process(float delta)
    {
        if (!canFire) 
        {
           fireTimer += delta;
           if(fireTimer >= fireSpeed) {
               fireTimer = 0;
               canFire = true;
           }
        }
    }
}
