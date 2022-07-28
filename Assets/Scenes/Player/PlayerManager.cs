using Godot;
using System;

public class PlayerManager : Node
{

    [Signal]
    delegate void OnHealthChanged(int health);

    [Signal]
    delegate void OnArmorChanged(int armor);

    Area areaCollider;
    WeaponManager weaponManager;

    AudioStreamPlayer hurtSound;

    const int maxHealth = 100;
    const int maxArmor = 100;

    int health = 100;
    int armor = 0;

    public int Health { get => health; set => SetHealth(value); }
    public int Armor { get => armor; set => SetArmor(value); }

    private void SetArmor(int value)
    {
        armor = Mathf.Clamp(value, 0, maxArmor);
        EmitSignal(nameof(OnArmorChanged), Armor);
    }

    private void SetHealth(int value)
    {
        health = Mathf.Clamp(value, 0, maxHealth);
        EmitSignal(nameof(OnHealthChanged), Health);
    }

    public override void _Ready()
    {
        weaponManager = GetNode<WeaponManager>("../Head/Hand");
        hurtSound = GetNode<AudioStreamPlayer>("HurtSound");
        areaCollider = GetNode<Area>("../CollisionShape/AreaCollider");
    }

    public void TakeDamage(int damage)
    {
        GD.Print("Player took damage: " + damage);
        Health = Health - damage;
        hurtSound.Stop();
        hurtSound.Play();
    }

    public override void _Process(float delta)
    {

    }

    public override void _PhysicsProcess(float delta)
    {
        CheckCollisions();
    }

    private void CheckCollisions()
    {
        var areas = areaCollider.GetOverlappingAreas();
        foreach (Area area in areas)
        {
            if (area is Pickup)
            {
                if (area is WeaponPickup weaponPickup)
                {
                    weaponManager.PickupWeapon(weaponPickup.WeaponType, weaponPickup.Ammo);
                }
                else if (area is AmmoPickup ammoPickup)
                {
                    weaponManager.PickupAmmo(ammoPickup.WeaponType, ammoPickup.Ammo);
                }
                else if (area is HealthPickup healthPickup)
                {
                    if (Health >= maxHealth)
                    {
                        return;
                    }
                    Health = Health + healthPickup.Health;
                }
                else if (area is ArmorPickup armorPickup)
                {
                    if (Armor >= maxArmor)
                    {
                        return;
                    }
                    Armor = Armor + armorPickup.Armor;
                }

                area.QueueFree();
            }
        }
    }
}
