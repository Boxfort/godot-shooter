using Godot;
using System;

public class ExplosiveBarrel : Carryable, Damageable
{
    bool hasExploded = false;
    int health = 1;
    public int Health => health;

    public override string InteractString => "PICK UP BARREL";

    PackedScene explosion;

    public override void _Ready()
    {
        explosion = GD.Load("res://Assets/Scenes/Explosion.tscn") as PackedScene;
    }

    public void TakeDamage(int damage, float knockback, Vector3 fromPosition)
    {
        if (hasExploded)
            return;

        health -= damage;

        if (health <= 0)
        {
            Explode();
            hasExploded = true;
        }
    }

    private void Explode()
    {
        if (hasExploded)
            return;

        Explosion instance = explosion.Instance() as Explosion;
        GetTree().Root.AddChild(instance);
        var transform = GlobalTransform;
        transform.origin += new Vector3(0, 1.5f, 0);
        instance.GlobalTransform = transform;
        instance.Explode();
        QueueFree();
    }

    public override void OnCarry()
    {
        base.OnCarry();
        GD.Print("I'm being carried!");
    }
}
