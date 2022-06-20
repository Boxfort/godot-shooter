using Godot;
using System;

public class ArmorPickup: Pickup
{
    [Export]
    int armor = 0;

    public int Armor { get => armor; }

    public override void _Ready()
    {
        base._Ready(); 
    }

    public override void _Process(float delta)
    {
        base._Process(delta); 
    }
}
