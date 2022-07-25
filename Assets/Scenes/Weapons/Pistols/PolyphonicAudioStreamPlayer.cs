using Godot;
using System;
using System.Collections.Generic;

public class PolyphonicAudioStreamPlayer : AudioStreamPlayer
{
    Queue<AudioStreamPlayer> sounds = new Queue<AudioStreamPlayer>();

    public void Play()
    {
        if (!Playing)
        {
            base.Play();
        }
        else
        {
            AudioStreamPlayer asp = (AudioStreamPlayer)Duplicate((int)DuplicateFlags.UseInstancing);
            asp.Connect("finished", this, nameof(OnSoundFinished));
            AddChild(asp);
            asp.Stream = Stream;
            asp.Play();
            sounds.Enqueue(asp);
        }
    }

    private void OnSoundFinished()
    {
        lock (sounds)
        {
            sounds.Dequeue().QueueFree();
        }
    }
}
