using Godot;
using System;

public class PlayerAreaCollider : Area, Damageable
{
    [Signal]
    delegate void OnDamageTaken(int damage);
    [Signal]
    delegate void OnKnockback(float knockback, Vector3 fromPosition);

    public void TakeDamage(int damage, float knockback, Vector3 fromPosition)
    {
        EmitSignal(nameof(OnDamageTaken), damage);
        EmitSignal(nameof(OnKnockback), knockback, fromPosition);
    }

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {

    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(float delta)
    {

    }
}
