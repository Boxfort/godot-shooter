using Godot;
using System.Collections.Generic;
using System.Linq;

public enum WeaponType
{
    None,
    Shotgun,
    Revolvers,
    BaseballBat,
    GrenadeLauncher
}

public class WeaponManager : Spatial
{
    [Signal]
    delegate void OnAmmoChanged(int ammoCount);

    [Signal]
    delegate void OnWeaponEquipped(WeaponType weaponType, int ammoCount);

    Spatial weaponSlot;
    WeaponType equippedWeaponType = WeaponType.None;
    Weapon equippedWeapon;

    Dictionary<WeaponType, PackedScene> weapons = new Dictionary<WeaponType, PackedScene> { };

    Dictionary<WeaponType, bool> hasWeapon = new Dictionary<WeaponType, bool> {
        { WeaponType.None, true },
        { WeaponType.Shotgun, true},
        { WeaponType.Revolvers, true },
        { WeaponType.BaseballBat, true },
        { WeaponType.GrenadeLauncher, true }
    };

    Dictionary<WeaponType, int> ammoCount = new Dictionary<WeaponType, int> {
        { WeaponType.None, -1 },
        { WeaponType.Shotgun, 10 },
        { WeaponType.Revolvers, 10 },
        { WeaponType.BaseballBat, -1 },
        { WeaponType.GrenadeLauncher, 100 },
    };

    public override void _Ready()
    {
        weaponSlot = GetNode<Spatial>("WeaponSlot");

        weapons.Add(WeaponType.Shotgun, GD.Load<PackedScene>("res://Assets/Scenes/Weapons/Shotgun/Shotgun.tscn"));
        weapons.Add(WeaponType.Revolvers, GD.Load<PackedScene>("res://Assets/Scenes/Weapons/Revolvers/Revolvers.tscn"));
        weapons.Add(WeaponType.BaseballBat, GD.Load<PackedScene>("res://Assets/Scenes/Weapons/BaseballBat/BaseballBat.tscn"));
        weapons.Add(WeaponType.GrenadeLauncher, GD.Load<PackedScene>("res://Assets/Scenes/Weapons/GrenadeLauncher/GrenadeLauncher.tscn"));
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(float delta)
    {
        if (Input.IsActionJustPressed("weapon1"))
        {
            EquipWeapon(WeaponType.BaseballBat);
        }
        if (Input.IsActionJustPressed("weapon2"))
        {
            EquipWeapon(WeaponType.Revolvers);
        }
        if (Input.IsActionJustPressed("weapon3"))
        {
            EquipWeapon(WeaponType.Shotgun);
        }
        if (Input.IsActionJustPressed("weapon4"))
        {
            EquipWeapon(WeaponType.GrenadeLauncher);
        }

        if (Input.IsActionPressed("fire"))
        {
            if (equippedWeapon != null && equippedWeapon.CanFire && EquippedWeaponHasAmmo())
            {
                equippedWeapon.Fire();
                if (ammoCount[equippedWeaponType] > 0)
                {
                    ammoCount[equippedWeaponType] -= 1;
                }
                EmitSignal(nameof(OnAmmoChanged), ammoCount[equippedWeaponType]);
            }
        }
    }

    private bool EquippedWeaponHasAmmo()
    {
        return ammoCount[equippedWeaponType] > 0 || ammoCount[equippedWeaponType] == -1;
    }

    public int GetAmmoCount(WeaponType weapon)
    {
        return ammoCount[weapon];
    }

    public List<WeaponType> GetWeapons()
    {
        return hasWeapon
            .ToList()
            .Where(x => x.Value)
            .Select(x => x.Key)
            .ToList();
    }

    public void PickupWeapon(WeaponType weapon, int ammo)
    {
        hasWeapon[weapon] = true;
        PickupAmmo(weapon, ammo);
        EquipWeapon(weapon);
    }

    public void PickupAmmo(WeaponType weapon, int ammo)
    {
        if (ammoCount[weapon] != -1)
        {
            ammoCount[weapon] += ammo;
            EmitSignal(nameof(OnAmmoChanged), ammoCount[weapon]);
        }
    }

    public void EquipWeapon(WeaponType weapon)
    {
        if (hasWeapon[weapon] && equippedWeaponType != weapon)
        {
            // Remove the active weapon before equipping
            Node activeWeapon = weaponSlot.GetChildOrNull<Node>(0);
            if (activeWeapon != null)
            {
                activeWeapon.QueueFree();
            }

            // Equip the weapon
            var weaponObject = weapons[weapon].Instance();
            weaponSlot.AddChild(weaponObject);
            equippedWeaponType = weapon;
            equippedWeapon = weaponObject as Weapon;
            equippedWeapon.Equip();

            EmitSignal(nameof(OnWeaponEquipped), equippedWeaponType, ammoCount[equippedWeaponType]);
        }
    }

}
