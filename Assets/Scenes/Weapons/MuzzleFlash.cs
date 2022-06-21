using Godot;
using System;

public class MuzzleFlash : Spatial
{
    Random rng = new Random();

    float flashTime = 0.1f;
    float flashTimer = 0.0f;
    bool shouldFlash = false;

    public void Flash()
    {
        RotateY(rng.Next(0, 6));
        var scale = (float)(0.02 * (rng.NextDouble() + 0.5));
        Scale = new Vector3(scale, scale, scale);
        Show();
    }

    public override void _Process(float delta)
    {
        if (Visible)
        {
            flashTimer += delta;

            if (flashTimer >= flashTime)
            {
                Hide();
                flashTimer = 0;
            }
        }
    }
}
