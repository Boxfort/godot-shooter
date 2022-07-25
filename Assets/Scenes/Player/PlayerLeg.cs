using Godot;
using System;
using System.Collections.Generic;

public class PlayerLeg : Spatial
{
    AnimationPlayer animationPlayer;
    AudioStreamPlayer kickAudioPlayer;
    List<AudioStream> kickSounds = new List<AudioStream>();
    Random rng = new Random();

    float kickTime = 0.5f;
    float kickImpactTime = 0.15f;
    float kickTimer = 0.0f;
    bool kicking = false;
    bool hit = false;

    public bool Kicking { get => kicking; }

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
        kickAudioPlayer = GetNode<AudioStreamPlayer>("AudioStreamPlayer");

        // TODO: Don't hardcode sounds somehow?
        kickSounds.Add(GD.Load<AudioStream>("res://Assets/Sounds/swing01.wav"));
        kickSounds.Add(GD.Load<AudioStream>("res://Assets/Sounds/swing02.wav"));
        kickSounds.Add(GD.Load<AudioStream>("res://Assets/Sounds/swing03.wav"));
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(float delta)
    {
        if (Kicking)
        {
            kickTimer += delta;

            if (kickTimer >= kickImpactTime && !hit)
            {
                hit = true;
                DoKickCollisions();
            }

            if (kickTimer >= kickTime)
            {
                kicking = false;
                kickTimer = 0;
            }
        }
    }

    public void Kick()
    {
        kicking = true;
        kickTimer = 0;
        animationPlayer.Stop();
        animationPlayer.Play("Kick");

        kickAudioPlayer.Stream = kickSounds[rng.Next(0, kickSounds.Count)];
        kickAudioPlayer.Play();
    }

    private void DoKickCollisions()
    {

    }

    private void ResetKick()
    {
        kicking = false;
        kickTimer = 0;
        hit = false;
    }
}
