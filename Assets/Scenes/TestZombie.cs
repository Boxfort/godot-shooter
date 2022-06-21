using Godot;
using System;
using System.Collections.Generic;

public class TestZombie : KinematicBody, Damageable
{
    Navigation navigation;
    Vector3[] path = new Vector3[0];
    int pathNode = 0;

    Spatial player;

    float moveSpeed = 3;

    int health = 25;

    public int Health => health;

    public void TakeDamage(int damage)
    {
        GD.Print("Taking damage: " + damage);
    }

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        try {
        navigation = GetParent<Navigation>();
        } catch (Exception) { }
        player = GetNode<Spatial>("../../Player");
    }

    float updatePathTime = 1.0f;
    float updatePathTimer = 0f;

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(float delta)
    {
        if (updatePathTimer >= updatePathTime) 
        {
            try {
            MoveTo(player.GlobalTransform.origin);
            }
            catch(Exception) {}
            updatePathTimer = 0;
        } 
        else 
        {
            updatePathTimer += delta;
        }

    }

    public override void _PhysicsProcess(float delta)
    {
        if (pathNode < path.Length) 
        {
            var direction = (path[pathNode]) - GlobalTransform.origin;
            if (direction.Length() < 0.1)
            {
                pathNode += 1;
            } 
            else
            {
                MoveAndSlide((direction.Normalized() * moveSpeed), Vector3.Up);
            }
        }
    }

    public void MoveTo(Vector3 targetPosition) 
    {
        path = navigation.GetSimplePath(GlobalTransform.origin, targetPosition);
        pathNode = 0;
    }
}
