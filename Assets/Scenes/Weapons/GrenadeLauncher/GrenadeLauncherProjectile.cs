using Godot;
using System;

public class GrenadeLauncherProjectile : KinematicBody
{
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
            // TODO: Boom
            QueueFree();
        }
        else
        {
            explosionTimer += delta;
        }
    }
}
