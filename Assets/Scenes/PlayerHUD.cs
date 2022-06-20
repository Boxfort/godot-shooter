using Godot;
using System;

public class PlayerHUD : CanvasLayer
{
    Label health;
    Label armor;
    Label weapon;
    Label ammo;
    WeaponManager weaponManager;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        weapon = GetNode<Label>("DebugPanel/Weapon");
        ammo = GetNode<Label>("DebugPanel/Ammo");
        weaponManager = GetNode<WeaponManager>("../Head/Hand");

        weaponManager.Connect("OnAmmoChanged", this, nameof(OnAmmoChanged));
        weaponManager.Connect("OnWeaponEquipped", this, nameof(OnWeaponEquipped));
    }


    private void OnAmmoChanged(int ammoCount) 
    {
        ammo.Text = ammoCount.ToString();
    }
    
    private void OnWeaponEquipped(WeaponType weaponType, int ammoCount) 
    {
        ammo.Text = ammoCount.ToString();
        weapon.Text = weaponType.ToString();
    }
}
