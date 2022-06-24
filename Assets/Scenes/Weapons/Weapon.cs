using System;
using Godot;
using Godot.Collections;

public abstract class Weapon : Spatial
{
    public abstract bool CanFire { get; }

    public abstract void Fire();

    public abstract void Equip();
}