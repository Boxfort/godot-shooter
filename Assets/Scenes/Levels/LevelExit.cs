using Godot;
using System;

public class LevelExit : StaticBody, Interactable
{
    [Export]
    bool requiresKey = false;

    [Export]
    KeyType keyType = KeyType.Red;

    public string InteractString => "EXIT LEVEL";

    public void Interact()
    {
        GD.Print("Finish Level");
    }
}
