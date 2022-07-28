using Godot;

public abstract class Carryable : KinematicBody, Interactable
{
    protected float gravity = 1.0f;
    protected bool shouldBounce = false;
    protected float velocity = 0;
    protected float velocityDecay = 10;
    protected float friction = 0;
    protected bool hasBeenThrown = false;
    protected bool isFalling = false;

    protected Vector3 gravityVec = Vector3.Zero;
    protected Vector3 direction = Vector3.Zero;
    Vector3 snap = Vector3.Zero;

    public abstract string InteractString { get; }

    virtual public void Interact() { }
    virtual public void OnCollide(KinematicCollision collision) { }
    virtual public void OnCarry()
    {
        isFalling = false;
    }

    virtual public void OnThrow(Vector3 direction, float force)
    {
        GD.Print("I've been thrown.");
        hasBeenThrown = true;
        isFalling = true;
        this.direction = direction;
        velocity = force;
    }

    virtual public void OnDrop(Vector3 direction, float force)
    {
        GD.Print("I've been dropped.");
        isFalling = true;
        this.direction = direction;
        velocity = force;
    }

    public override void _PhysicsProcess(float delta)
    {
        if (isFalling)
            HandleFalling(delta);
    }

    private void HandleFalling(float delta)
    {
        gravityVec += Vector3.Down * gravity * delta;
        var movement = (direction * velocity * delta) + gravityVec;
        var collision = MoveAndCollide(movement);
        bool onFloor = false;
        Vector3 lastRemainder = Vector3.Zero;

        if (collision != null)
        {
            float dotProduct = Vector3.Up.Dot(collision.Normal);
            lastRemainder = collision.Remainder;

            if (shouldBounce)
            {
                var reflect = collision.Remainder.Bounce(collision.Normal);
                direction = movement.Bounce(collision.Normal).Normalized();
                MoveAndCollide(reflect);

                if (dotProduct > 0)
                {
                    gravityVec *= 1 - dotProduct;
                }
            }

            var maxSlope = Mathf.Deg2Rad(45);

            if (collision.Normal.AngleTo(Vector3.Up) <= maxSlope)
            {
                if (((PhysicsBody)collision.Collider).GetCollisionLayerBit(0))
                    onFloor = true;

                if (!shouldBounce)
                {
                    gravityVec = Vector3.Zero;
                }
            }
        }

        velocity = Mathf.Max(velocity - (velocityDecay * delta), 0);

        GD.Print(onFloor);
        GD.Print(lastRemainder.Length());
        if (onFloor && lastRemainder.Length() < 0.1)
        {
            isFalling = false;
            GD.Print("I've stopped falling!");
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