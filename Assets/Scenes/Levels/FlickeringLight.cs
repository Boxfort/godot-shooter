using Godot;
using System;

public class FlickeringLight : SpotLight
{
    Random rng = new Random();

    float minFlickerTime = 0.1f;
    float maxFlickerTime = 1f;

    float flickerTimer = 0.0f;
    float flickerTime = 0.0f;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        generateRandomFlickerTime();
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(float delta)
    {
        if (flickerTimer >= flickerTime)
        {
            if (LightEnergy == 1)
            {
                LightEnergy = 0;
            }
            else
            {
                LightEnergy = 1;
            }

            flickerTimer = 0;
            generateRandomFlickerTime();
        }

        flickerTimer += delta;
    }

    private void generateRandomFlickerTime()
    {
        flickerTime = (float)rng.NextDouble() * (maxFlickerTime - minFlickerTime) + minFlickerTime;
    }
}
