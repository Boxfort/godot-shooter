using Godot;
using System;

public class PlayerHUD : CanvasLayer
{
    Label health;
    Label armor;
    Label weapon;
    Label ammo;
    WeaponManager weaponManager;
    PlayerManager playerManager;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        weapon = GetNode<Label>("DebugPanel/Weapon");
        ammo = GetNode<Label>("UI/Ammo/AmmoCount");
        health = GetNode<Label>("UI/Health/HealthCount");
        armor = GetNode<Label>("UI/Armor/ArmorCount");

        weaponManager = GetNode<WeaponManager>("../Head/Hand");
        playerManager = GetNode<PlayerManager>("../PlayerManager");

        weaponManager.Connect("OnAmmoChanged", this, nameof(OnAmmoChanged));
        weaponManager.Connect("OnWeaponEquipped", this, nameof(OnWeaponEquipped));
        playerManager.Connect("OnHealthChanged", this, nameof(OnHealthChanged));
        playerManager.Connect("OnArmorChanged", this, nameof(OnArmorChanged));

        OnHealthChanged(playerManager.Health);
        OnArmorChanged(playerManager.Armor);
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

    private void OnHealthChanged(int newHealth)
    {
        health.Text = newHealth.ToString();
    }

    private void OnArmorChanged(int newArmor)
    {
        armor.Text = newArmor.ToString();
    }
}
