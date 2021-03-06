using Godot;
using System;
using System.Collections.Generic;

public class CharacterController : KinematicBody, Damageable
{
    Spatial head;
    Spatial hand;
    PlayerLeg leg;
    ShakeableCamera camera;
    Camera gunCamera;
    CollisionShape collisionShape;
    CollisionShape areaCollisionShape;
    Area areaCollider;
    PlayerManager playerManager;
    RayCast canStand;
    Area headInWater;

    AudioStreamPlayer jumpAudio;
    ImprovedAudioStreamPlayer stepAudioPlayer;

    const float footstepTime = 0.5f;
    float footstepTimer = 0;

    float headRollDegreesMax = 5;
    float headRollSpeed = 7.5f;

    float groundAcceleration = 7f;
    float airAcceleration = 3f;
    float moveSpeed = 14f;
    float mouseSensitivity = 0.2f;

    float currentKnockback = 0.0f;
    Vector3 knockbackDirection = Vector3.Zero;

    const float damageCooldown = 0.25f;
    float damageCooldownTimer = 0;

    const float maxFallTraumaTime = 1.5f; // The time after which the camera trauma will be at its max when landing.
    float fallTime = 0.0f;

    const float gravity = 40;
    const float jumpVelocity = 13;
    const float terminalVelocity = 40f;
    const float waterTerminalVelocity = 2f;

    bool autoJump = true;
    bool jumpQueued = false;

    bool inWater = false;
    bool isHeadInWater = false;

    Vector3 snap;
    Vector3 inputDirection = Vector3.Zero;
    Vector3 velocity = new Vector3();
    Vector3 gravityVec = new Vector3();
    Vector3 movement = new Vector3();
    Vector3 currentVelocity = Vector3.Zero;
    Vector2 mouseMovement = Vector2.Zero;

    float handSwayStrength = 0.1f;
    float handSwayMaxX = 0.1f;
    float handSwayMaxY = 0.05f;
    float handSwaySmoothing = 6f;
    float maxHeadPitch = 88;

    float currentAcceleration;

    public override void _Ready()
    {
        head = GetNode<Spatial>("Head");
        hand = GetNode<Spatial>("Head/Hand");
        collisionShape = GetNode<CollisionShape>("CollisionShape");
        areaCollider = collisionShape.GetNode<Area>("AreaCollider");
        areaCollisionShape = areaCollider.GetNode<CollisionShape>("CollisionShape");
        canStand = collisionShape.GetNode<RayCast>("CanStand");
        headInWater = collisionShape.GetNode<Area>("HeadInWater");
        camera = head.GetNode<ShakeableCamera>("ShakeableCamera");
        gunCamera = hand.GetNode<Camera>("ViewportContainer/Viewport/GunCamera");
        playerManager = GetNode<PlayerManager>("PlayerManager");

        jumpAudio = GetNode<AudioStreamPlayer>("JumpSound");
        stepAudioPlayer = GetNode<ImprovedAudioStreamPlayer>("StepSound");

        // TODO: an actual solution for setting world environment
        var environment = GetNode<WorldEnvironment>("../WorldEnvironment");
        if (environment != null)
        {
            GD.Print("Setting gun camera environment...");
            gunCamera.Environment = environment.Environment;
        }

        currentAcceleration = groundAcceleration;

        Input.MouseMode = Input.MouseModeEnum.Captured;
    }

    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventMouseMotion mouseMotion)
        {
            float xMovement = -mouseMotion.Relative.x * mouseSensitivity;
            float yMovement = -mouseMotion.Relative.y * mouseSensitivity;
            mouseMovement = new Vector2(xMovement, yMovement);

            RotateY(Mathf.Deg2Rad(mouseMovement.x));
            head.RotateX(Mathf.Deg2Rad(mouseMovement.y));
            Vector3 rotDeg = head.RotationDegrees;
            rotDeg.x = Mathf.Clamp(rotDeg.x, -maxHeadPitch, maxHeadPitch);
            rotDeg.y = 0;
            head.RotationDegrees = rotDeg;
        }
    }

    public override void _Process(float delta)
    {
        gunCamera.GlobalTransform = camera.GlobalTransform;

        // Weapon sway
        float xSway = Mathf.Clamp(mouseMovement.x * handSwayStrength, -handSwayMaxX, handSwayMaxX);
        float ySway = Mathf.Clamp(mouseMovement.y * handSwayStrength, -handSwayMaxY, handSwayMaxY);
        Vector3 swayOffset = new Vector3(xSway, ySway, 0);
        hand.Translation = hand.Translation.LinearInterpolate(swayOffset, delta * handSwaySmoothing);
        // Resets the mouse movement every frame so we can tell if it has stopped moving.
        mouseMovement = Vector2.Zero;

        if (damageCooldownTimer < damageCooldown)
            damageCooldownTimer += delta;
    }

    public override void _PhysicsProcess(float delta)
    {
        float horizontalRotation = GlobalTransform.basis.GetEuler().y;
        float forwardInput = Input.GetActionStrength("move_backward") - Input.GetActionStrength("move_forward");
        float strafeInput = Input.GetActionStrength("move_right") - Input.GetActionStrength("move_left");
        inputDirection = new Vector3(strafeInput, 0, forwardInput).Rotated(Vector3.Up, horizontalRotation).Normalized();

        CheckCollisions();
        QueueJump();
        Crouch();

        if (inWater)
        {
            HandleWaterMovement(delta);
        }
        else
        {
            HandleMovement(delta);
        }

        HandleFootsteps(delta);
        HandleKnockback(delta);
        MovePlayer(delta);
        RollHead(horizontalRotation, delta);
    }

    private void HandleFootsteps(float delta)
    {
        if (IsOnFloor() && !inWater && inputDirection.Length() > 0.1)
        {
            footstepTimer -= delta;
            if (footstepTimer <= 0)
            {
                stepAudioPlayer.Play();
                footstepTimer = footstepTime;
            }
        }
    }

    private void HandleKnockback(float delta)
    {
        if (currentKnockback > 0)
        {
            currentKnockback -= (gravity * 4) * delta;

            if (currentKnockback <= 0)
            {
                currentKnockback = 0;
            }
        }
    }

    private void HandleMovement(float delta)
    {
        if (IsOnFloor() && !inWater)
        {
            snap = -GetFloorNormal();
            currentAcceleration = groundAcceleration;
            gravityVec = Vector3.Zero;

            if (fallTime > 0)
            {
                camera.AddTrauma(NormalizedInRange(fallTime, 0.5f, maxFallTraumaTime, 0, 2.0f));
                fallTime = 0.0f;
            }
        }
        else
        {
            if (IsOnCeiling())
            {
                // If we bonk our head, start going down.
                gravityVec = Vector3.Zero;
                velocity.y = 0;
            }
            else
            {
                gravityVec += Vector3.Down * gravity * delta;
            }

            currentAcceleration = airAcceleration;
            snap = Vector3.Down;
            fallTime += delta;
        }

        if (jumpQueued && IsOnFloor())
        {
            snap = Vector3.Zero;
            gravityVec.y = jumpVelocity;

            // Maintain velocity from moving platforms.
            gravityVec += GetFloorVelocity();

            if (!jumpAudio.Playing)
            {
                jumpAudio.Play();
            }
        }
    }

    private void HandleWaterMovement(float delta)
    {
        fallTime = 0.0f;
        snap = Vector3.Zero;
        currentAcceleration = airAcceleration;

        // If we hit the water going quickly we want to slow gradually.
        if (gravityVec.y < -waterTerminalVelocity)
        {
            gravityVec.y += (gravity * 2) * delta;
        }
        else
        {
            gravityVec += Vector3.Down * (gravity / 3) * delta;
            gravityVec.y = Mathf.Clamp(gravityVec.y, -waterTerminalVelocity, waterTerminalVelocity);
        }

        // Rotate input based on our forward look direction so we can move up and down in the water by looking.
        var headTransform = head.GlobalTransform;
        inputDirection = inputDirection.Rotated(headTransform.basis.x.Normalized(), headTransform.basis.GetEuler().x);

        // Let the player go up and down using jump/crouch
        if (Input.IsActionPressed("jump"))
        {
            inputDirection.y = 1;
        }
        else if (Input.IsActionPressed("crouch"))
        {
            inputDirection.y = -0.5f;
        }
    }

    private void MovePlayer(float delta)
    {
        velocity = velocity.LinearInterpolate(inputDirection * moveSpeed, currentAcceleration * delta);
        gravityVec.y = Mathf.Clamp(gravityVec.y, -terminalVelocity, terminalVelocity);
        movement = velocity + gravityVec + (currentKnockback * knockbackDirection);
        currentVelocity = MoveAndSlideWithSnap(movement, snap, Vector3.Up);
    }


    private void CheckCollisions()
    {
        var areas = areaCollider.GetOverlappingAreas();
        bool touchedWater = false;

        foreach (Area area in areas)
        {
            // TODO: remove effects after testing
            if (area.IsInGroup("water"))
            {
                touchedWater = true;
                if (!inWater)
                {
                    GD.Print("Entered the water");
                    inWater = true;
                }
            }

            if (area.IsInGroup("killbox"))
            {
                GD.Print("DEAD");
            }
        }

        if (!touchedWater && inWater)
        {
            inWater = false;
        }

        // Head in water collsion
        if (inWater)
        {
            bool headTouchedWater = headInWater.GetOverlappingAreas().Count > 0;

            if (!isHeadInWater && headTouchedWater)
            {
                OnHeadEnterWater();
            }
            else if (isHeadInWater && !headTouchedWater)
            {
                OnHeadExitWater();
            }
        }
        else
        {
            if (isHeadInWater)
            {
                OnHeadExitWater();
            }
        }
    }

    private void OnHeadEnterWater()
    {
        isHeadInWater = true;
        AudioServer.GetBusIndex("Master");
        AudioServer.AddBusEffect(0, new AudioEffectLowPassFilter(), 0);
    }

    private void OnHeadExitWater()
    {
        isHeadInWater = false;
        AudioServer.RemoveBusEffect(0, 0);
        AudioServer.GetBusIndex("Master");
    }

    private void RollHead(float horizontalRotation, float delta)
    {
        Vector3 camRotDeg = head.RotationDegrees;
        var normalizedRotation = NormalizedInRange(-currentVelocity.Rotated(Vector3.Down, horizontalRotation).x, -moveSpeed, moveSpeed, -headRollDegreesMax, headRollDegreesMax);
        camRotDeg.z = Mathf.Lerp(camRotDeg.z, normalizedRotation, headRollSpeed * delta);
        camRotDeg.x = Mathf.Clamp(camRotDeg.x, -maxHeadPitch, maxHeadPitch);
        camRotDeg.y = 0;
        head.RotationDegrees = camRotDeg;
    }

    private void Crouch()
    {
        if (Input.IsActionPressed("crouch"))
        {
            ((CapsuleShape)collisionShape.Shape).Height = 0.01f;
            ((CapsuleShape)areaCollisionShape.Shape).Height = 0.01f;
            var transform = collisionShape.Transform;
            transform.origin.y = 0.5f;
            collisionShape.Transform = transform;
        }
        else if (!canStand.IsColliding())
        {
            ((CapsuleShape)collisionShape.Shape).Height = 1.0f;
            ((CapsuleShape)areaCollisionShape.Shape).Height = 1.0f;
            var transform = collisionShape.Transform;
            transform.origin.y = 0;
            collisionShape.Transform = transform;
        }
    }

    private void QueueJump()
    {
        if (autoJump)
        {
            jumpQueued = Input.IsActionPressed("jump");
        }
        else
        {
            jumpQueued = Input.IsActionJustPressed("jump") && !Input.IsActionJustReleased("jump");
        }
    }

    private float NormalizedInRange(float number, float oldMin, float oldMax, float newMin, float newMax)
    {
        return (newMax - newMin) * (number - oldMin) / (oldMax - oldMin) + newMin;
    }

    public void TakeDamage(int damage, float knockback, Vector3 fromPosition)
    {
        if (damageCooldownTimer < damageCooldown)
            return;

        playerManager.TakeDamage(damage);
        knockbackDirection = -GlobalTransform.origin.DirectionTo(fromPosition);
        currentKnockback += knockback;
        camera.AddTrauma(0.1f * damage);
        damageCooldownTimer = 0;
    }
}