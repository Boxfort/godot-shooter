using Godot;
using System;
using System.Collections.Generic;

public class BaseballBat : WeaponMelee
{
    protected override int Damage => 5;
    protected override float Knockback => 30.0f;

    protected override float AttackTime => 0.4f;

    protected override float AttackHitStartTime => 0.1f;

    protected override float AttackHitEndTime => 0.2f;

    AudioStreamPlayer hitAudio;
    AudioStreamPlayer swingAudio;
    List<AudioStream> swingSounds = new List<AudioStream>();
    Random rng = new Random();


    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        base._Ready();

        hitAudio = GetNode<AudioStreamPlayer>("HitAudio");
        swingAudio = GetNode<AudioStreamPlayer>("SwingAudio");

        // TODO: Don't hardcode sounds somehow?
        swingSounds.Add(GD.Load<AudioStream>("res://Assets/Sounds/swing01.wav"));
        swingSounds.Add(GD.Load<AudioStream>("res://Assets/Sounds/swing02.wav"));
        swingSounds.Add(GD.Load<AudioStream>("res://Assets/Sounds/swing03.wav"));
    }

    public override void Fire()
    {
        base.Fire();

        swingAudio.Stream = swingSounds[rng.Next(0, swingSounds.Count)];
        swingAudio.Play();
    }

    protected override void OnAttackHit()
    {
        hitAudio.Play();
    }

    //  // Called every frame. 'delta' is the elapsed time since the previous frame.
    //  public override void _Process(float delta)
    //  {
    //      
    //  }
}
