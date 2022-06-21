using Godot;
using System;

public class ExplosiveBarrel : Spatial, Damageable
{
    int health = 15;
    public int Health => health;

    PackedScene explosion;

    public override void _Ready()
    {
        explosion = GD.Load("res://Assets/Scenes/Explosion.tscn") as PackedScene;
    }

    public void TakeDamage(int damage)
    {
        health -= damage;

        if (health <= 0)
        {
            Explode();
        }
    }

    private void Explode() 
    {
        Explosion instance = explosion.Instance() as Explosion;
        GetTree().Root.AddChild(instance);
        var transform = GlobalTransform;
        transform.origin += new Vector3(0, 1.5f, 0);
        instance.GlobalTransform = transform;
        instance.Explode();
        QueueFree();
    }
}
