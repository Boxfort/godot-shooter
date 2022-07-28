using Godot;
using System;
using System.Collections.Generic;

public class ImprovedAudioStreamPlayer : AudioStreamPlayer
{
    Queue<ImprovedAudioStreamPlayer> soundInstances = new Queue<ImprovedAudioStreamPlayer>();

    [Export]
    bool polyphonic = false;

    [Export]
    bool randomPitch = false;

    [Export(PropertyHint.Range, "0,2")]
    float pitchMin = 0.9f;

    [Export(PropertyHint.Range, "0,2")]
    float pitchMax = 1.1f;

    [Export]
    bool randomSound = false;

    [Export]
    List<AudioStream> sounds = new List<AudioStream>();

    Random rng = new Random();


    public void Play()
    {
        if ((!Playing && polyphonic) || !polyphonic)
        {
            if (randomSound)
            {
                Stream = sounds[rng.Next(0, sounds.Count)];
            }

            if (randomPitch)
            {
                PitchScale = (float)rng.NextDouble() * (pitchMax - pitchMin) + pitchMax;
            }

            base.Play();
        }
        else if (polyphonic)
        {
            ImprovedAudioStreamPlayer asp = (ImprovedAudioStreamPlayer)Duplicate((int)DuplicateFlags.UseInstancing | (int)DuplicateFlags.Scripts);
            asp.Connect("finished", this, nameof(OnSoundFinished));
            AddChild(asp);
            asp.Play();
            soundInstances.Enqueue(asp);
        }
    }

    private void OnSoundFinished()
    {
        lock (soundInstances)
        {
            soundInstances.Dequeue().QueueFree();
        }
    }
}
