using Godot;

public abstract class Carryable : KinematicBody, Interactable
{
    [Signal]
    public delegate void StopCarrying();

    const float terminalVelocity = 2f;
    protected const float gravity = 1.0f;

    [Export]
    protected bool shouldBounce = true;
    protected float velocity = 0;
    protected float velocityDecay = 20;
    [Export]
    protected float friction = 0;
    protected bool hasBeenThrown = false;
    protected bool isFalling = true;

    protected Vector3 gravityVec = Vector3.Zero;
    protected Vector3 direction = Vector3.Zero;
    Vector3 snap = Vector3.Zero;
    float maxSlope = Mathf.Deg2Rad(45);

    public abstract string InteractString { get; }

    virtual public void Interact() { }
    virtual public void OnCollide(KinematicCollision collision) { }
    virtual public void OnCarry()
    {
        isFalling = false;
        gravityVec = Vector3.Zero;
    }

    virtual public void OnThrow(Vector3 direction, float force)
    {
        hasBeenThrown = true;
        isFalling = true;
        this.direction = direction;
        velocity = force;
    }

    virtual public void OnDrop(Vector3 direction, float force)
    {
        isFalling = true;
        this.direction = direction;
        velocity = force;
    }

    public override void _PhysicsProcess(float delta)
    {
        if (isFalling)
            HandleFalling(delta);
    }

    Vector3 lastRemainder = Vector3.Zero;

    private void HandleFalling(float delta)
    {
        gravityVec += Vector3.Down * gravity * delta;
        gravityVec.y = Mathf.Clamp(gravityVec.y, -terminalVelocity, terminalVelocity);
        var movement = (direction * velocity * delta) + gravityVec;
        var collision = MoveAndCollide(movement);
        bool onFloor = false;

        if (collision != null)
        {
            OnCollide(collision);

            float dotProduct = Vector3.Up.Dot(collision.Normal);
            lastRemainder = collision.Remainder;

            var reflect = collision.Remainder.Bounce(collision.Normal);
            direction = movement.Bounce(collision.Normal).Normalized();


            if (collision.Normal.AngleTo(Vector3.Up) <= maxSlope)
            {
                // We're only on the floor if we're colliding with the world. We don't want to stop on top of somethings head.
                if (((PhysicsBody)collision.Collider).GetCollisionLayerBit(0) && !(collision.Collider is Carryable))
                {
                    onFloor = true;
                    velocity = Mathf.Max(velocity - (friction * delta), 0);
                    if (!shouldBounce)
                    {
                        gravityVec = Vector3.Zero;
                        direction.y = 0;
                        reflect.y = 0;
                    }
                }

                if (!shouldBounce)
                    reflect.y = 0;
            }

            MoveAndCollide(reflect);

            if (shouldBounce && dotProduct > 0)
            {
                gravityVec *= 1 - dotProduct;
            }
        }

        velocity = Mathf.Max(velocity - (velocityDecay * delta), 0);

        if (onFloor && lastRemainder.Length() <= 0.01)
        {
            isFalling = false;
        }
    }

    protected void HandleGravity(float delta)
    {
        if (IsOnFloor())
        {
            snap = -GetFloorNormal();
            gravityVec = Vector3.Zero;
        }
        else
        {
            snap = Vector3.Down;
            gravityVec += Vector3.Down * gravity * delta;
        }
    }
}