using Godot;
using System;

public class Shotgun : Weapon
{
    AnimationPlayer animationPlayer;

    bool canFire = true;
    float fireSpeed = 1f;
    float fireTimer = 0;

    public override bool CanFire => canFire;

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
    }

    public override void _Ready()
    {
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
