using UnityEngine;
using UnityEngine.InputSystem;

// Comments

/// <summary>
/// Controller script for a player's spaceship, which uses player input to move the spaceship and shoot.
/// Author: Luke Lepkowski (lpl6448@rit.edu)
/// </summary>
public class PlayerSpaceship : Spaceship
{
    public UIHealthBar uiHealthBar;

    /// <summary>
    /// The ship's forward acceleration (units/s^2)
    /// </summary>
    [Header("Physics")]
    public float acceleration;

    /// <summary>
    /// The base angular acceleration (degrees/s^2)
    /// </summary>
    public float baseTurnAcceleration;

    /// <summary>
    /// Max speed at which the ship's velocity will align itself with the forward direction (degrees/s^2)
    /// </summary>
    public float baseTurnToVelocity;

    /// <summary>
    /// Drag coefficient of the space the ship is traveling through
    /// </summary>
    public float drag;

    /// <summary>
    /// Angular drag coefficient of the space the ship is traveling through
    /// </summary>
    public float angularDrag;

    /// <summary>
    /// When braking, the damper put on the ship's velocity
    /// </summary>
    public float brakeDamper;

    /// <summary>
    /// When braking, the amount of deceleration of the ship (units/s^2)
    /// </summary>
    public float brakeDeceleration;

    public float brakeAngularDamper;

    public float brakeAngularDeceleration;

    /// <summary>
    /// When braking, the max speed at which the ship's velocity will align itself with the forward direction (degrees/s^2)
    /// </summary>
    public float brakingTurnToVelocity;

    /// <summary>
    /// When braking, the added angular acceleration that the ship can turn at (degrees/s^2)
    /// </summary>
    public float brakingTurnAcceleration;

    /// <summary>
    /// Bounce coefficient that velocity is multiplied by when a collision with a map wall occurs
    /// </summary>
    public float wallBounceCoefficient;

    /// <summary>
    /// Prefab of the bullet to instantiate when this ship shoots
    /// </summary>
    [Header("Shooting")]
    public Bullet bulletPrefab;

    /// <summary>
    /// Array containing Transforms (used for initial positions and rotations of bullets)
    /// A bullet is instantiated using each Transform (meaning that multiple bullets may be shot at once)
    /// </summary>
    public Transform[] bulletSpawnPoints;

    /// <summary>
    /// The maximum rate that bullets can be shot at if the player is rapidly clicking or tapping space
    /// </summary>
    public float clickShootRate;

    /// <summary>
    /// The rate that bullets are shot at if the player is holding left mouse button or space
    /// </summary>
    public float holdShootRate;

    /// <summary>
    /// Backwards impulse given to this Spaceship whenever bullet are shot
    /// </summary>
    public float shootRecoil;

    /// <summary>
    /// Raw input for whether the player is applying the brake
    /// </summary>
    private bool rawBrakingInput;

    /// <summary>
    /// Vector2 representing the raw movement input from the player
    /// </summary>
    private Vector2 rawMovementInput;

    /// <summary>
    /// Whether this ship is current shooting or not (whether the player is holding the shoot button)
    /// </summary>
    private bool shooting;

    /// <summary>
    /// In-game time of the last shot taken from this ship, used for regulating shoot rates
    /// </summary>
    private float lastShootTime = -1;

    /// <summary>
    /// When the game starts, add this spaceship to the physics simulation
    /// </summary>
    private void Start()
    {
        physicsWorld.AddObject(this);
    }

    private void OnDestroy()
    {
        physicsWorld.RemoveObject(this);
    }

    /// <summary>
    /// The Update function runs a physics simulation, taking into account user input to move
    /// the ship and update its position and rotation on the screen.
    /// </summary>
    private void Update()
    {
        UpdateFlashes();
        uiHealthBar.fillAmount = health / maxHealth;

        // If stunned, ignore any player input
        Vector2 currentMovementInput = !stunned ? rawMovementInput : Vector2.zero;
        bool currentBrakingInput = !stunned ? rawBrakingInput : false;

        // If the player is pressing a brake key AND the ship is moving forward, brake
        currentBrakingInput = currentBrakingInput || currentMovementInput.y < -0.5f;
        bool isBraking = Vector2.Dot(transform.up, velocity) > 0 && currentBrakingInput;
        bool isAccelerating = !currentBrakingInput && currentMovementInput.sqrMagnitude > 0;

        // Angularly accelerate or decelerate the ship
        angularVelocity += baseTurnAcceleration * -currentMovementInput.x * Time.deltaTime;
        if (isBraking)
        {
            angularVelocity += brakingTurnAcceleration * velocity.magnitude * -currentMovementInput.x * Time.deltaTime;
            angularVelocity *= Mathf.Clamp01(1 - brakeAngularDamper * Time.deltaTime - brakeAngularDeceleration / angularVelocity * Time.deltaTime);
        }

        // Apply angular drag
        angularVelocity *= Mathf.Clamp01(1 - Mathf.Abs(angularVelocity) * angularDrag * Time.deltaTime);

        // Accelerate or brake the ship
        if (isAccelerating)
        {
            velocity += (Vector2)transform.up * currentMovementInput.y * acceleration * Time.deltaTime;
            if (velocity.sqrMagnitude > 0.01f)
            {
                MoveVelocityTowardDirection(baseTurnToVelocity * velocity.magnitude * Time.deltaTime);
            }
        }
        else if (isBraking)
        {
            if (velocity.sqrMagnitude > 0.01f)
            {
                velocity *= Mathf.Clamp01(1 - brakeDamper * Time.deltaTime - brakeDeceleration / velocity.magnitude * Time.deltaTime);
            }
            MoveVelocityTowardDirection(brakingTurnToVelocity * Time.deltaTime);
        }

        // Apply drag
        velocity *= Mathf.Clamp01(1 - velocity.magnitude * drag * Time.deltaTime);

        // Perform a physics tick using the new velocities
        PhysicsTick(Time.deltaTime);

        // Shoot if necessary
        if (!stunned && shooting && Time.time - lastShootTime >= 1 / holdShootRate)
        {
            Shoot();
        }
    }

    /// <summary>
    /// Utility function that moves the current velocity's direction a maximum number of degrees
    /// toward the current forward direction of the ship
    /// </summary>
    /// <param name="maxTurnToVelocity">Maximum degrees that the ship can turn during this frame</param>
    private void MoveVelocityTowardDirection(float maxTurnToVelocity)
    {
        float speed = velocity.magnitude;
        Vector2 dir = velocity / speed;

        float currentAngle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg + 90;
        float goalAngle = transform.eulerAngles.z + 180;
        float newAngle = Mathf.MoveTowardsAngle(currentAngle, goalAngle, maxTurnToVelocity);

        dir = new Vector2(Mathf.Sin(newAngle * Mathf.Deg2Rad), -Mathf.Cos(newAngle * Mathf.Deg2Rad));
        velocity = dir * speed;
    }

    /// <summary>
    /// Shoots bullets by instantiating a bullet at each Transform in bulletSpawnPoints
    /// </summary>
    private void Shoot()
    {
        // Instantiate bullets
        foreach (Transform spawnPoint in bulletSpawnPoints)
        {
            Bullet bullet = Instantiate(bulletPrefab, spawnPoint.position, spawnPoint.rotation);
            bullet.Origin = this;
            physicsWorld.AddObject(bullet);
        }

        // Recoil
        velocity -= (Vector2)transform.up * shootRecoil;

        lastShootTime = Time.time;
    }

    /// <summary>
    /// Called by the PhysicsWorld whenever this object collides with the map boundaries, causing this ship to bounce
    /// </summary>
    /// <param name="point">World-space point on this object's collider circle where a collision was detected</param>
    /// <param name="normal">World-space normal of the collision (out from the point on this object's collider circle)</param>
    public override void OnWallCollision(Vector2 point, Vector2 normal)
    {
        // Correct the ship's position to the edge of the wall
        transform.position = (Vector3)(point + normal * worldCircleRadius - worldCircleCenter) + transform.position;

        // If the ship is outside the bounds and is moving back toward the game area, we don't need to bounce
        if (Vector2.Dot(velocity, normal) < 0)
        {
            // Bounce the ship off the wall
            velocity = Vector2.Reflect(velocity, -normal);
            float speedRelativeToNormal = -Vector2.Dot(velocity, normal);
            velocity += normal * speedRelativeToNormal * (1 - wallBounceCoefficient);
        }
    }

    /// <summary>
    /// Called by the PhysicsWorld whenever this object collides with another PhysicsObject.
    /// A response only occurs if this ship collides with an EnemySpaceship, temporarily stunning this ship.
    /// </summary>
    /// <param name="otherObj">PhysicsObject that this object collided with</param>
    /// <param name="point">World-space point on this object's collider circle where a collision was detected</param>
    /// <param name="normal">World-space normal of the collision (out from the point on this object's collider circle)</param>
    public override void OnObjectCollision(PhysicsObject otherObj, Vector2 point, Vector2 normal)
    {
        if (otherObj is EnemySpaceship)
        {
            StunFromCollision(normal);
        }
    }

    /// <summary>
    /// Called by a PlayerInput component to update the player's movement input
    /// </summary>
    /// <param name="context">CallbackContext representing the move action (Vector2)</param>
    public void OnMove(InputAction.CallbackContext context)
    {
        rawMovementInput = context.ReadValue<Vector2>();
    }

    /// <summary>
    /// Called by a PlayerInput component to update whether the player is applying the brakes
    /// </summary>
    /// <param name="context">CallbackContext representing the brake action (float)</param>
    public void OnBrake(InputAction.CallbackContext context)
    {
        rawBrakingInput = context.ReadValue<float>() > 0;
    }

    /// <summary>
    /// Called by a PlayerInput component to update whether the player is shooting, possibly causing the
    /// ship to shoot and updating the shooting field
    /// </summary>
    /// <param name="context">CallbackContext representing the shoot action</param>
    public void OnShoot(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            shooting = true;
            if (!stunned && Time.time - lastShootTime >= 1 / clickShootRate)
            {
                Shoot();
            }
        }
        else if (context.canceled)
        {
            shooting = false;
        }
    }
}