using Godot;
using Godot.Collections;

public abstract class WeaponProjectile : Weapon
{
    [Export]
    protected PackedScene projectile;
    protected AnimationPlayer animationPlayer;

    protected bool canFire = false;
    protected float fireRate = 0.75f;
    protected float fireTimer = 0.0f;
    protected bool equipped = false;

    public override bool CanFire => canFire;
    public override bool Equipped { get => equipped; set { equipped = value; canFire = value; } }

    public override void Equip()
    {
        Show();
        animationPlayer.Play("Equip");
    }

    private void OnAnimationFinished(string animationName)
    {
        if (animationName == "Equip")
        {
            canFire = true;
            equipped = true;
        }
    }

    public override void _Ready()
    {
        Hide();
        animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");

        animationPlayer.Connect("animation_finished", this, nameof(OnAnimationFinished));
    }

    public override void _Process(float delta)
    {
        if (!canFire && equipped)
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
}