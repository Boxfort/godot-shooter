using Godot;

public class Explosion : TraumaCauser
{
    AnimatedSprite3D sprite;
    CollisionShape collisionShape;
    AudioStreamPlayer3D explosionAudio;

    int damage = 15;
    float radius = 3.5f;
    float knockback = 15.0f;

    public int Damage { get => damage; set => damage = value; }
    public float Knockback { get => knockback; set => knockback = value; }
    public float Radius { get => radius; set => SetRadius(value); }

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        collisionShape = GetNode<CollisionShape>("CollisionShape");
        sprite = GetNode<AnimatedSprite3D>("AnimatedSprite3D");
        sprite.Connect("animation_finished", this, nameof(OnAnimationFinished));
        explosionAudio = GetNode<AudioStreamPlayer3D>("AudioStreamPlayer3D");
    }

    private void OnAnimationFinished()
    {
        QueueFree();
    }

    bool shouldExplode = false;
    float explosionDelayTimer = 0;
    float explosionDelay = 0.1f;

    public override void _PhysicsProcess(float delta)
    {
        // We need to wait for the physics engine so we can actually check
        // for overlapping bodies after creating the object.
        if (shouldExplode)
        {
            if (explosionDelayTimer >= explosionDelay)
            {
                DoExplosion();
                shouldExplode = false;
            }
            else
            {
                explosionDelayTimer += delta;
            }
        }
    }

    public void Explode()
    {
        shouldExplode = true;
    }

    public void DoExplosion()
    {
        sprite.Play("Explode");
        explosionAudio.Play();
        CauseTrauma(1.5f);
        var areaCollisions = GetOverlappingAreas();
        var bodyCollisions = GetOverlappingBodies();
        areaCollisions = GetOverlappingAreas();
        bodyCollisions = GetOverlappingBodies();

        foreach (CollisionObject area in areaCollisions)
        {
            bodyCollisions.Add(area);
        }

        foreach (Node collision in bodyCollisions)
        {
            if (collision is Damageable damageable)
            {
                damageable.TakeDamage(Damage, knockback, GlobalTransform.origin);
            }
        }
    }

    private void SetRadius(float value)
    {
        ((SphereShape)collisionShape.Shape).Radius = value;
        radius = value;
    }
}
