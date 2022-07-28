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
        RotateX(rng.Next(0, 6));
        var scale = (float)(0.4 * (rng.NextDouble() + 0.8));
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
