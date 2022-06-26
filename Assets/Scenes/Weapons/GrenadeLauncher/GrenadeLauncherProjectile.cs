using Godot;
using System;

public class GrenadeLauncherProjectile : KinematicBody
{
    PackedScene explosion;

    float initialVelocity = 30.0f;
    float velocityDecay = 10.0f;
    float gravity = 1.0f;
    Vector3 gravityVec = Vector3.Zero;
    float velocity;

    Vector3 direction = Vector3.Zero;
    Vector3 rotationVec = Vector3.Zero;
    float rotationSpeed = 1.0f;

    float explosionTime = 2.0f;
    float explosionTimer = 0f;

    public Vector3 Direction { get => direction; set => direction = value; }

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        velocity = initialVelocity;
        explosion = (PackedScene)GD.Load("res://Assets/Scenes/Explosion.tscn");
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _PhysicsProcess(float delta)
    {
        gravityVec += Vector3.Down * gravity * delta;
        //RotationDegrees += (rotationVec * rotationSpeed) * delta;
        var movement = (direction * velocity * delta) + gravityVec;
        var collision = MoveAndCollide(movement);

        if (collision != null)
        {
            if (collision.Collider is Damageable damageable)
            {
                Explode();
            }

            var reflect = collision.Remainder.Bounce(collision.Normal);
            direction = movement.Bounce(collision.Normal).Normalized();
            MoveAndCollide(reflect);

            float dotProduct = Vector3.Up.Dot(collision.Normal);

            GD.Print(((Node)collision.Collider).Name);

            if (dotProduct > 0)
            {
                gravityVec *= 1 - dotProduct;
                //GD.Print("BOUNCE UP");
            }
        }


        velocity = Mathf.Max(velocity - (velocityDecay * delta), 0);
        GD.Print(velocity);

        if (explosionTimer >= explosionTime)
        {
            Explode();
        }
        else
        {
            explosionTimer += delta;
        }
    }

    private void Explode()
    {
        Explosion instance = (Explosion)explosion.Instance();

        instance.Knockback = 40.0f;

        var transform = GlobalTransform;
        transform.origin += Vector3.Up * 1.5f;
        instance.GlobalTransform = transform;
        GetTree().Root.AddChild(instance);
        instance.Explode();
        QueueFree();
    }
}
