using Godot;
using System;

public class SoldierBullet : KinematicBody
{
    float speed = 50.0f;
    Vector3 direction = Vector3.Zero;
    int damage = 5;
    float knockback = 20.0f;
    public float Speed { get => speed; set => speed = value; }
    public Vector3 Direction { get => direction; set => direction = value.Normalized(); }
    public int Damage { get => damage; set => damage = value; }
    public float Knockback { get => knockback; set => knockback = value; }

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {

    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(float delta)
    {
        KinematicCollision collision = MoveAndCollide(direction * speed * delta);

        if (collision != null)
        {
            Node collider = (Node)collision.Collider;

            if (collider.IsInGroup("player") || collider is Damageable)
            {
                ((Damageable)collider).TakeDamage(Damage, Knockback, GlobalTransform.origin);
            }

            QueueFree();
        }
    }
}
