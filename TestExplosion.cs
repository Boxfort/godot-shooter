using Godot;
using System;

public class TestExplosion : TraumaCauser
{
    AnimatedSprite3D sprite;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        sprite = GetNode<AnimatedSprite3D>("AnimatedSprite3D");
        sprite.Connect("animation_finished", this, nameof(OnExplosion));
    }

    private void OnExplosion()
    {
        CauseTrauma(1.5f);
    }
}
