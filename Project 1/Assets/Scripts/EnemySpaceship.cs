using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Controller script for an enemy spaceship, which uses an AI to move the spaceship and shoot at a target.
/// Author: Luke Lepkowski (lpl6448@rit.edu)
/// </summary>
public class EnemySpaceship : Spaceship
{
    /// <summary>
    /// Transform that this ship will follow and shoot at
    /// </summary>
    public Transform Target;

    /// <summary>
    /// The ship's forward acceleration (units/s^2)
    /// </summary>
    [Header("Physics")]
    public float acceleration;

    /// <summary>
    /// The angular spring toward the direction of the Target
    /// </summary>
    public float angularSpring;

    /// <summary>
    /// Drag coefficient of the space the ship is traveling through
    /// </summary>
    public float drag;

    /// <summary>
    /// Angular drag coefficient of the space the ship is traveling through
    /// </summary>
    public float angularDrag;

    /// <summary>
    /// When chasing, the minimum distance that this ship tries to keep from the Target
    /// </summary>
    public float chaseBrakeDistance;

    /// <summary>
    /// When fleesing, the minimum distance that this ship tries to keep from the Target
    /// </summary>
    public float fleeMinBrakeDistance;

    /// <summary>
    /// When fleesing, the maximum distance that this ship tries to keep from the Target
    /// </summary>
    public float fleeMaxBrakeDistance;

    /// <summary>
    /// When braking, the damper put on the ship's velocity
    /// </summary>
    public float brakeDamper;

    /// <summary>
    /// When braking, the amount of deceleration of the ship (units/s^2)
    /// </summary>
    public float brakeDeceleration;

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
    /// The rate that bullets are shot at while this ship is in a shooting burst
    /// </summary>
    public float shootRate;

    /// <summary>
    /// When chasing, the probability (0-1) per second that this ship begins a shooting burst
    /// </summary>
    public float chaseShootBurstChancePerSecond;

    /// <summary>
    /// When fleeing, the probability (0-1) per second that this ship begins a shooting burst
    /// </summary>
    public float fleeShootBurstChancePerSecond;

    /// <summary>
    /// Minimum number of bullets to shoot in one shooting burst (inclusive)
    /// </summary>
    public int shootBurstMin;

    /// <summary>
    /// Maximum number of bullets to shoot in one shooting burst (inclusive)
    /// </summary>
    public int shootBurstMax;

    /// <summary>
    /// Backwards impulse given to this Spaceship whenever bullet are shot
    /// </summary>
    public float shootRecoil;

    /// <summary>
    /// Current mode of the ship (true if chasing, false if fleeing)
    /// </summary>
    [Header("AI")]
    private bool isChasing;

    /// <summary>
    /// Probability (0-1) per second that this ship swaps mode (by inverting isChasing)
    /// </summary>
    public float modeSwitchChancePerSecond;

    /// <summary>
    /// Minimum number of seconds between mode swaps
    /// </summary>
    public float modeMinimumSeconds;

    /// <summary>
    /// Whether this ship is currently shooting bullets in a burst
    /// </summary>
    private bool isShootingBurst;

    /// <summary>
    /// In-game time of the last mode swap, used to enforce modeMinimumSeconds
    /// </summary>
    private float lastModeChange;

    /// <summary>
    /// When the game starts, add this spaceship to the physics simulation
    /// </summary>
    private void Start()
    {
        physicsWorld.AddObject(this);
    }

    /// <summary>
    /// The Update function runs a physics simulation, using the AI to move
    /// the ship and shoot, then running a physics tick to update the position and rotation.
    /// </summary>
    private void Update()
    {
        UpdateFlashes();

        // Compute relevant information for the Target of this ship
        Vector2 targetOffset = Target.position - transform.position;
        float targetDis = targetOffset.magnitude;
        Vector2 targetDir = targetOffset / targetDis;
        float targetTurn = Vector2.SignedAngle(transform.up, targetDir);

        // When the ship is shooting or stunned, it loses control of acceleration and turning
        if (!isShootingBurst && !stunned)
        {
            if (isChasing)
            {
                // If it is chasing the player, accelerate toward the player and brake once it gets too close
                if (targetDis < chaseBrakeDistance)
                {
                    if (velocity.sqrMagnitude > 0.01f)
                    {
                        velocity *= Mathf.Clamp01(1 - brakeDamper * Time.deltaTime - brakeDeceleration / velocity.magnitude * Time.deltaTime);
                    }
                }
                else
                {
                    velocity += (Vector2)transform.up * acceleration * Time.deltaTime;
                }
            }
            else
            {
                // If fleeing from the player, accelerate or brake as needed to remain between the flee min and max distances
                if (targetDis > fleeMinBrakeDistance && targetDis < fleeMaxBrakeDistance)
                {
                    if (velocity.sqrMagnitude > 0.01f)
                    {
                        velocity *= Mathf.Clamp01(1 - brakeDamper * Time.deltaTime - brakeDeceleration / velocity.magnitude * Time.deltaTime);
                    }
                }
                else
                {
                    velocity += (Vector2)transform.up * acceleration * Time.deltaTime;

                    // Turn away from the target if the ship is too close
                    if (targetDis < fleeMinBrakeDistance)
                    {
                        targetTurn *= -1;
                    }
                }
            }

            // Angularly accelerate or decelerate the ship
            angularVelocity += angularSpring * targetTurn * Time.deltaTime;
        }

        // Apply drag
        velocity *= Mathf.Clamp01(1 - velocity.magnitude * drag * Time.deltaTime);

        // Apply angular drag
        angularVelocity *= Mathf.Clamp01(1 - Mathf.Abs(angularVelocity) * angularDrag * Time.deltaTime);

        // Perform a physics tick
        PhysicsTick(Time.deltaTime);

        // Random chance of performing a shooting burst
        if (!isShootingBurst && Vector2.Dot(transform.up, targetDir) > 0.8f)
        {
            float shootBurstChancePerSecond = isChasing ? chaseShootBurstChancePerSecond : fleeShootBurstChancePerSecond;
            float shootBurstChancePerFrame = 1 - Mathf.Pow(1 - shootBurstChancePerSecond, Time.deltaTime);
            if (Random.value < shootBurstChancePerFrame)
            {
                StartCoroutine(ShootBurst(Random.Range(shootBurstMin, shootBurstMax + 1)));
            }
        }

        // Random chance of changing mode from chasing to fleeing or vice versa
        if (Time.time - lastModeChange >= modeMinimumSeconds)
        {
            float modeChangeChancePerFrame = 1 - Mathf.Pow(1 - modeSwitchChancePerSecond, Time.deltaTime);
            if (Random.value < modeChangeChancePerFrame)
            {
                isChasing = !isChasing;
                lastModeChange = Time.time;
            }
        }
    }

    /// <summary>
    /// Unity coroutine to execute a shooting burst, shooting bullets at the shootRate
    /// until the given number of shots occur or until the shooting burst is canceled
    /// </summary>
    /// <param name="shots">Number of shots that will occur in this shooting burst (unless canceled)</param>
    /// <returns>IEnumerator used for the coroutine</returns>
    private IEnumerator ShootBurst(int shots)
    {
        isShootingBurst = true;
        for (int i = 0; i < shots && isShootingBurst; i++)
        {
            Shoot();
            yield return new WaitForSeconds(1 / shootRate);

            if (stunned)
            {
                break;
            }
        }
        isShootingBurst = false;
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
    }


    /// <summary>
    /// Called by the PhysicsWorld whenever this object collides with the map boundaries, causing this ship to bounce
    /// </summary>
    /// <param name="point">World-space point on this object's collider circle where a collision was detected</param>
    /// <param name="normal">World-space normal of the collision (out from the point on this object's collider circle)</param>
    public override void OnWallCollision(Vector2 point, Vector2 normal)
    {
        // If the ship is outside the bounds and is moving back toward the game area, we don't need to teleport it
        if (Vector2.Dot(velocity, normal) < 0)
        {
            // Correct the ship's position to the edge of the wall
            transform.position = (Vector3)(point + normal * worldCircleRadius - worldCircleCenter) + transform.position;

            // Bounce the ship off the wall
            velocity = Vector2.Reflect(velocity, -normal);
            float speedRelativeToNormal = -Vector2.Dot(velocity, normal);
            velocity += normal * speedRelativeToNormal * (1 - wallBounceCoefficient);
        }
    }

    /// <summary>
    /// Called by the PhysicsWorld whenever this object collides with another PhysicsObject.
    /// A response only occurs if this ship collides with an PlayerSpaceship, temporarily stunning this ship.
    /// </summary>
    /// <param name="otherObj">PhysicsObject that this object collided with</param>
    /// <param name="point">World-space point on this object's collider circle where a collision was detected</param>
    /// <param name="normal">World-space normal of the collision (out from the point on this object's collider circle)</param>
    public override void OnObjectCollision(PhysicsObject otherObj, Vector2 point, Vector2 normal)
    {
        if (otherObj is PlayerSpaceship)
        {
            StunFromCollision(normal);
        }
    }
}