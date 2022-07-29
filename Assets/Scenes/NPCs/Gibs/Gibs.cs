using Godot;
using System;

public class Gibs : Spatial
{
    AudioStreamPlayer3D gibAudio;
    Particles gibsParticles;
    Gib gib1;
    Gib gib2;
    Gib gib3;

    const float fadeBeginTime = 3.0f;
    float fadeBeginTimer = 0f;

    float gibScale = 1.0f;
    float fadeSpeed = 1.0f;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        gibAudio = GetNode<AudioStreamPlayer3D>("AudioStreamPlayer3D");
        gibAudio.Play();

        gibsParticles = GetNode<Particles>("Particles");
        gibsParticles.Emitting = true;

        gib1 = GetNode<Gib>("gib");
        gib2 = GetNode<Gib>("gib2");
        gib3 = GetNode<Gib>("gib3");
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(float delta)
    {
        if (fadeBeginTimer < fadeBeginTime)
        {
            fadeBeginTimer += delta;
        }
        else
        {
            gibScale -= delta * fadeSpeed;

            gib1.SetScaleRelative(gibScale);
            gib2.SetScaleRelative(gibScale);
            gib3.SetScaleRelative(gibScale);

            if (gibScale <= 0)
            {
                QueueFree();
            }
        }
    }
}
