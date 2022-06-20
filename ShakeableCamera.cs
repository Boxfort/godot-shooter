using Godot;
using System;

public class ShakeableCamera : Area
{
    Camera camera;

    float traumaReductionRate = 1;
    float maxX = 10;
    float maxY = 10;
    float maxZ = 5;

    OpenSimplexNoise noise = new OpenSimplexNoise();
    float noiseSpeed = 50;
    float trauma = 0;
    float time = 0;

    Vector3 initialRotation;

    public override void _Ready()
    {
        camera = GetNode<Camera>("Camera");
        initialRotation = camera.RotationDegrees;
    }

    public override void _Process(float delta)
    {
        time += delta;
        trauma = Mathf.Max(trauma - delta * traumaReductionRate, 0);

        Vector3 cameraRotDeg = camera.RotationDegrees;
        cameraRotDeg.x = initialRotation.x + maxX * GetShakeIntensity() * GetNoiseFromSeed(1);
        cameraRotDeg.y = initialRotation.y + maxY * GetShakeIntensity() * GetNoiseFromSeed(2);
        cameraRotDeg.z = initialRotation.z + maxZ * GetShakeIntensity() * GetNoiseFromSeed(3);
        camera.RotationDegrees = cameraRotDeg;
    }

    public void AddTrauma(float amount) 
    {
        trauma = Mathf.Clamp(trauma + amount, 0, 2);
    }

    private float GetShakeIntensity() 
    {
        return trauma * trauma;
    }

    private float GetNoiseFromSeed(int _seed) 
    {
        noise.Seed = _seed;
        return noise.GetNoise1d(time * noiseSpeed);
    }
}
