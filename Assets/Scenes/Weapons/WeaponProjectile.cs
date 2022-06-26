using Godot;
using Godot.Collections;

public abstract class WeaponProjectile : Weapon
{
    [Export]
    protected PackedScene projectile;
    protected AnimationPlayer animationPlayer;

    protected bool canFire;

    public override bool CanFire => canFire;

    public override void Equip()
    {
        Show();
        animationPlayer.Play("Equip");
    }

    public override void _Ready()
    {
        Hide();
        animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
    }
}