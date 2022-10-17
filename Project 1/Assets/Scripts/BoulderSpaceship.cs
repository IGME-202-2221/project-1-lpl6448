using UnityEngine;

/// <summary>
/// Controller script for the Boulder, which uses an AI to move the boulder/asteroid randomly
/// around the map and to shoot occasional asteroid shards in random directions.
/// Author: Luke Lepkowski (lpl6448@rit.edu)
/// </summary>
public class BoulderSpaceship : EnemySpaceship
{
    /// <summary>
    /// Reference to the ship's circular health bar
    /// </summary>
    public CircularHealthBar healthBar;

    /// <summary>
    /// The acceleration (units/s^2) of this ship
    /// </summary>
    [Header("Physics")]
    public float acceleration;

    /// <summary>
    /// The maximum angular acceleration (degrees/s^2) of this ship
    /// </summary>
    public float angularAcceleration;

    /// <summary>
    /// The timestep (or value that Time.time is multiplied by) in the angular acceleration noise
    /// (to make the boulder randomly turn back and forth)
    /// </summary>
    public float angularAccelerationTimestep;

    /// <summary>
    /// Drag coefficient of the space the ship is traveling through
    /// </summary>
    public float drag;

    /// <summary>
    /// Angular drag coefficient of the space the ship is traveling through
    /// </summary>
    public float angularDrag;

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
    /// Probability (0-1) per second that this ship shoots a bunch of bullets
    /// </summary>
    public float shootChancePerSecond;

    /// <summary>
    /// Minimum (inclusive) number of bullets shot in one burst
    /// </summary>
    public int minBulletsShot;

    /// <summary>
    /// Maximum (inclusive) number of bullets shot in one burst
    /// </summary>
    public int maxBulletsShot;

    /// <summary>
    /// The amount of damage that this ship must take before it shoots a bunch of bullets
    /// </summary>
    public float damagePerShoot;

    /// <summary>
    /// Backwards impulse given to this Spaceship whenever bullet are shot
    /// </summary>
    public float shootRecoil;

    /// <summary>
    /// Probability (0-1) per second that the boulder begins to move in a different direction
    /// </summary>
    [Header("AI")]
    public float moveDirSwitchChancePerSecond;

    /// <summary>
    /// Current direction that this boulder is aiming to move in
    /// </summary>
    private Vector2 moveDir;

    /// <summary>
    /// Vertical noise offset in Perlin noise to make each boulder rotate at different speeds and in different directions
    /// </summary>
    private float angularAccelerationNoiseOffset;

    /// <summary>
    /// Amount of damage this boulder has taken since the last shot burst, used to make the boulder shoot
    /// out more fragments while taking damage
    /// </summary>
    private float accumulatedDamage = 0;

    /// <summary>
    /// At initialization, set the angular acceleration noise offset and initialize the move direction
    /// </summary>
    private void Awake()
    {
        angularAccelerationNoiseOffset = Random.value * 100 - 50;
        ChangeMoveDirection();
    }

    /// <summary>
    /// Every frame, accelerate this ship in moveDir, occasionally shoot out asteroid fragments,
    /// and perform a physics tick.
    /// </summary>
    private void Update()
    {
        UpdateFlashes();
        healthBar.fillAmount = health / maxHealth;

        if (!dead && !stunned)
        {
            // Random chance of changing the velocity direction
            float moveDirSwitchChancePerFrame = 1 - Mathf.Pow(1 - moveDirSwitchChancePerSecond, Time.deltaTime);
            if (Random.value < moveDirSwitchChancePerFrame)
            {
                ChangeMoveDirection();
            }

            // Move the ship toward a random direction
            velocity += moveDir * acceleration * Time.deltaTime;

            // Angularly accelerate the ship toward a random direction
            float angAccelNoise = Mathf.PerlinNoise(angularAccelerationNoiseOffset, Time.time * angularAccelerationTimestep);
            angularVelocity += (angAccelNoise * 2 - 1) * angularAcceleration * Time.deltaTime;

            // Random chance of shooting
            float shootChancePerFrame = 1 - Mathf.Pow(1 - shootChancePerSecond, Time.deltaTime);
            if (Random.value < shootChancePerFrame)
            {
                Shoot(Random.Range(minBulletsShot, maxBulletsShot + 1));
            }
        }

        // Apply drag
        velocity *= Mathf.Clamp01(1 - velocity.magnitude * drag * Time.deltaTime);

        // Apply angular drag
        angularVelocity *= Mathf.Clamp01(1 - Mathf.Abs(angularVelocity) * angularDrag * Time.deltaTime);

        // Perform a physics tick
        PhysicsTick(Time.deltaTime);
    }

    /// <summary>
    /// Changes moveDir to a new random direction, causing the boulder to move in a different direction
    /// </summary>
    private void ChangeMoveDirection()
    {
        float moveDirRad = Random.value * Mathf.PI * 2;
        moveDir = new Vector2(Mathf.Cos(moveDirRad), Mathf.Sin(moveDirRad));
    }

    /// <summary>
    /// Shoots the given number of bullets/fragments, each in a random direction
    /// </summary>
    /// <param name="count">Number of bullets/fragments to shoot</param>
    private void Shoot(int count)
    {
        for (int i = 0; i < count; i++)
        {
            // Instantiate bullet
            float shootRad = Random.value * Mathf.PI * 2;
            Vector2 shootDir = new Vector2(Mathf.Cos(shootRad), Mathf.Sin(shootRad));
            Bullet bullet = Instantiate(bulletPrefab, shootDir * worldCircleRadius + worldCircleCenter, Quaternion.identity);
            bullet.transform.up = shootDir;
            bullet.Origin = this;
            physicsWorld.AddObject(bullet);

            // Recoil
            velocity -= shootDir * shootRecoil;
        }
    }

    /// <summary>
    /// Damages this ship by the indicated amount of damage, playing the damageParticles and activating the damageFlash.
    /// The boulder also shoots bullets/fragments when taking damage.
    /// </summary>
    /// <param name="damage">Amount of damage that will be subtracted from this ship's health</param>
    /// <param name="addScore">Whether score should be added for this damage</param>
    /// <returns>Amount of damage done to the ship</returns>
    public override float Damage(float damage, bool addScore)
    {
        if (health > damage)
        {
            accumulatedDamage += damage;
            while (accumulatedDamage > damagePerShoot)
            {
                accumulatedDamage -= damagePerShoot;
                Shoot(Random.Range(minBulletsShot, maxBulletsShot + 1));
            }
        }

        return base.Damage(damage, addScore);
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

            ChangeMoveDirection();
        }
    }
}