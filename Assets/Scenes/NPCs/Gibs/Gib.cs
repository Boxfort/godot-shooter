using Godot;
using System;

public class Gib : KinematicBody
{
    Random rng = new Random();
    ImprovedAudioStreamPlayer3D impactAudio;

    float velocityDecay = 10.0f;
    float gravity = 1.0f;
    Vector3 gravityVec = Vector3.Zero;
    float velocity;

    Vector3 direction = Vector3.Zero;
    Vector3 rotationVec = Vector3.Zero;
    float rotationSpeed = 5.0f;

    const int maxBounces = 4;
    int bounces = 0;

    Vector3 initialScale;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        impactAudio = GetNode<ImprovedAudioStreamPlayer3D>("AudioStreamPlayer3D");

        direction = new Vector3(randomAxis(), 1.0f, randomAxis());
        GD.Print(direction);
        velocity = 15.0f;
        Scale *= (float)rng.NextDouble() + 0.5f;
        initialScale = Scale;
    }

    private float randomAxis()
    {
        return ((float)rng.NextDouble() * 2) - 1;
    }

    public void SetScaleRelative(float scale)
    {
        Scale = initialScale * scale;
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(float delta)
    {
        if (bounces >= maxBounces)
            return;

        gravityVec += Vector3.Down * gravity * delta;
        RotationDegrees += (rotationVec * rotationSpeed) * delta;
        var movement = (direction * velocity * delta) + gravityVec;
        var collision = MoveAndCollide(movement);

        if (collision != null)
        {
            impactAudio.Play();

            rotationVec = new Vector3(
                ((float)rng.NextDouble() * 360) - 180,
                ((float)rng.NextDouble() * 360) - 180,
                ((float)rng.NextDouble() * 360) - 180
            );

            bounces++;

            if (bounces >= maxBounces)
                return;

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
    }
}
