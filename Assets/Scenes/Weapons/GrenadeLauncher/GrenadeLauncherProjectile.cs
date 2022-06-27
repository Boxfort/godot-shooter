using Godot;
using System;

public class GrenadeLauncherProjectile : KinematicBody
{
    PackedScene explosion;
    Random rng = new Random();

    float initialVelocity = 40.0f;
    float velocityDecay = 10.0f;
    float gravity = 1.0f;
    Vector3 gravityVec = Vector3.Zero;
    float velocity;

    Vector3 direction = Vector3.Zero;
    Vector3 rotationVec = Vector3.Zero;
    float rotationSpeed = 5.0f;

    float explosionTime = 2.0f;
    float explosionTimer = 0f;


    public Vector3 Direction { get => direction; set => direction = value; }

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        velocity = initialVelocity;
        explosion = (PackedScene)GD.Load("res://Assets/Scenes/Explosion.tscn");

        // Check right away for a collision if we've shot at our feet, otherwise it'll just bounce off us.
        var collision = MoveAndCollide(Vector3.Zero, testOnly: true);
        if (collision != null)
        {
            ShouldExplode(collision);
        }
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _PhysicsProcess(float delta)
    {
        gravityVec += Vector3.Down * gravity * delta;
        RotationDegrees += (rotationVec * rotationSpeed) * delta;
        var movement = (direction * velocity * delta) + gravityVec;
        var collision = MoveAndCollide(movement);

        if (collision != null)
        {
            ShouldExplode(collision);

            rotationVec = new Vector3(
                ((float)rng.NextDouble() * 360) - 180,
                ((float)rng.NextDouble() * 360) - 180,
                ((float)rng.NextDouble() * 360) - 180
            );

            var reflect = collision.Remainder.Bounce(collision.Normal);
            direction = movement.Bounce(collision.Normal).Normalized();
            MoveAndCollide(reflect);

            float dotProduct = Vector3.Up.Dot(collision.Normal);

            if (dotProduct > 0)
            {
                gravityVec *= 1 - dotProduct;
            }
        }

        velocity = Mathf.Max(velocity - (velocityDecay * delta), 0);

        if (explosionTimer >= explosionTime)
        {
            Explode();
        }
        else
        {
            explosionTimer += delta;
        }

    }

    private void ShouldExplode(KinematicCollision collision)
    {
        if (collision.Collider is Damageable damageable || ((Node)collision.Collider).IsInGroup("player"))
        {
            Explode();
        }
    }

    public void Explode()
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
