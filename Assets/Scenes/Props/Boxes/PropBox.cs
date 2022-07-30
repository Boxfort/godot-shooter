using Godot;
using System;

public class PropBox : Carryable, Damageable
{
    [Export]
    PackedScene gibs;

    public override string InteractString => "PICKUP BOX";

    bool destroyed = false;
    public void TakeDamage(int damage, float knockback, Vector3 fromPosition)
    {
        if (destroyed)
            return;

        destroyed = true;
        EmitSignal(nameof(StopCarrying));

        Spatial instance = (Spatial)gibs.Instance();
        instance.GlobalTransform = GlobalTransform;
        GetTree().Root.AddChild(instance);

        QueueFree();
    }

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {

    }

    //  // Called every frame. 'delta' is the elapsed time since the previous frame.
    //  public override void _Process(float delta)
    //  {
    //      
    //  }
}
