using Godot;
using System;

public class Revolvers : Weapon
{
    AnimationPlayer animationPlayerLeft;
    AnimationPlayer animationPlayerRight;

    bool fireLeft = false;
    bool canFire = true;
    float fireSpeed = 0.2f;
    float fireTimer = 0;

    public override bool CanFire => canFire;

    public override void Equip()
    {
        animationPlayerLeft.Play("Equip");
        animationPlayerRight.Play("Equip");
        Show();
    }

    public override void Fire()
    {
        canFire = false;

        if (fireLeft) 
        {
            animationPlayerLeft.Stop(true);
            animationPlayerLeft.Play("Fire");
            fireLeft = false;
        } 
        else
        {
            animationPlayerRight.Stop(true);
            animationPlayerRight.Play("Fire");
            fireLeft = true;
        }
    }

    public override void _Ready()
    {
        Hide();
        animationPlayerLeft = GetNode<AnimationPlayer>("Revolver/AnimationPlayer");
        animationPlayerRight = GetNode<AnimationPlayer>("Revolver2/AnimationPlayer");

        animationPlayerLeft.Connect("animation_finished", this, nameof(OnAnimationFinished));
        animationPlayerRight.Connect("animation_finished", this, nameof(OnAnimationFinished));
    }

    private void OnAnimationFinished(String animationName) 
    {
        GD.Print("Animation Finished: " + animationName);
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
