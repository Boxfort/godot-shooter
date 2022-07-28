using Godot;
using System.Collections.Generic;
using System.Linq;

public enum WeaponType
{
    None,
    Shotgun,
    Pistols,
    AssaultRifle,
    BaseballBat,
    GrenadeLauncher
}

public class WeaponManager : Spatial
{
    [Signal]
    delegate void OnAmmoChanged(int ammoCount);

    [Signal]
    delegate void OnWeaponEquipped(WeaponType weaponType, int ammoCount);

    PlayerLeg playerLeg;
    InteractRayCast interactRayCast;

    Spatial weaponSlot;
    WeaponType equippedWeaponType = WeaponType.None;
    Weapon equippedWeapon;

    Dictionary<WeaponType, PackedScene> weapons = new Dictionary<WeaponType, PackedScene> { };

    Dictionary<WeaponType, bool> hasWeapon = new Dictionary<WeaponType, bool> {
        { WeaponType.None, true },
        { WeaponType.Shotgun, true},
        { WeaponType.Pistols, true },
        { WeaponType.AssaultRifle, true },
        { WeaponType.BaseballBat, true },
        { WeaponType.GrenadeLauncher, true }
    };

    Dictionary<WeaponType, int> ammoCount = new Dictionary<WeaponType, int> {
        { WeaponType.None, -1 },
        { WeaponType.Shotgun, 100 },
        { WeaponType.Pistols, 100 },
        { WeaponType.AssaultRifle, 100 },
        { WeaponType.BaseballBat, -1 },
        { WeaponType.GrenadeLauncher, 100 },
    };

    public override void _Ready()
    {
        weaponSlot = GetNode<Spatial>("WeaponSlot");
        playerLeg = GetNode<PlayerLeg>("Leg");
        interactRayCast = GetNode<InteractRayCast>("InteractRayCast");
        interactRayCast.Connect("OnBeginCarry", this, nameof(HideWeapon));
        interactRayCast.Connect("OnEndCarry", this, nameof(ShowWeapon));

        weapons.Add(WeaponType.Shotgun, GD.Load<PackedScene>("res://Assets/Scenes/Weapons/Shotgun/Shotgun.tscn"));
        weapons.Add(WeaponType.Pistols, GD.Load<PackedScene>("res://Assets/Scenes/Weapons/Pistols/Pistols.tscn"));
        weapons.Add(WeaponType.BaseballBat, GD.Load<PackedScene>("res://Assets/Scenes/Weapons/BaseballBat/BaseballBat.tscn"));
        weapons.Add(WeaponType.GrenadeLauncher, GD.Load<PackedScene>("res://Assets/Scenes/Weapons/GrenadeLauncher/GrenadeLauncher.tscn"));
        weapons.Add(WeaponType.AssaultRifle, GD.Load<PackedScene>("res://Assets/Scenes/Weapons/AssaultRifle/AssaultRifle.tscn"));
    }


    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(float delta)
    {
        if (!interactRayCast.IsCarrying)
        {
            if (Input.IsActionJustPressed("weapon1"))
            {
                EquipWeapon(WeaponType.BaseballBat);
            }
            if (Input.IsActionJustPressed("weapon2"))
            {
                EquipWeapon(WeaponType.Pistols);
            }
            if (Input.IsActionJustPressed("weapon3"))
            {
                EquipWeapon(WeaponType.Shotgun);
            }
            if (Input.IsActionJustPressed("weapon4"))
            {
                EquipWeapon(WeaponType.AssaultRifle);
            }
            if (Input.IsActionJustPressed("weapon5"))
            {
                EquipWeapon(WeaponType.GrenadeLauncher);
            }

            if (Input.IsActionPressed("kick") && !playerLeg.Kicking)
            {
                playerLeg.Kick();
            }
        }

        if (Input.IsActionJustPressed("interact"))
        {
            interactRayCast.DoInteract();
        }

        if (Input.IsActionPressed("fire"))
        {
            if (interactRayCast.IsCarrying)
            {
                interactRayCast.DoThrow();
                return;
            }

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

    private void HideWeapon()
    {
        if (equippedWeapon != null)
        {
            equippedWeapon.Hide();
            equippedWeapon.Equipped = false;
        }
    }

    private void ShowWeapon()
    {
        if (equippedWeapon != null)
        {
            equippedWeapon.Equip();
            equippedWeapon.Show();
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
            var children = weaponSlot.GetChildren();

            foreach (Node child in children)
            {
                child.QueueFree();
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
