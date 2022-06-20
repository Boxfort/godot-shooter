using Godot;
using System;

public class AmmoPickup : Pickup
{
    [Export]
    WeaponType weaponType = WeaponType.None;

    [Export]
    int ammo = 0;

    public WeaponType WeaponType { get => weaponType; set => weaponType = value; }
    public int Ammo { get => ammo; set => ammo = value; }

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        base._Ready(); 
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(float delta)
    {
        base._Process(delta); 
    }
}
