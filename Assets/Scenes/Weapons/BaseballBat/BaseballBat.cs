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
    ImprovedAudioStreamPlayer swingAudio;
    Random rng = new Random();


    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        base._Ready();

        hitAudio = GetNode<AudioStreamPlayer>("HitAudio");
        swingAudio = GetNode<ImprovedAudioStreamPlayer>("SwingAudio");
    }

    public override void Fire()
    {
        base.Fire();
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
