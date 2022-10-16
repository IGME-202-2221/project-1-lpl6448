using UnityEngine;

public class BoulderSpaceship : EnemySpaceship
{
    public CircularHealthBar healthBar;

    [Header("Physics")]
    public float acceleration;

    public float angularAcceleration;

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

    public float shootChancePerSecond;

    public int minBulletsShot;

    public int maxBulletsShot;

    public float damagePerShoot;

    /// <summary>
    /// Backwards impulse given to this Spaceship whenever bullet are shot
    /// </summary>
    public float shootRecoil;

    [Header("AI")]
    public float moveDirSwitchChancePerSecond;

    private Vector2 moveDir;

    private float angularAccelerationNoiseOffset;

    private float accumulatedDamage = 0;

    private void Awake()
    {
        angularAccelerationNoiseOffset = Random.value * 100 - 50;
        ChangeMoveDirection();
    }

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

    private void ChangeMoveDirection()
    {
        float moveDirRad = Random.value * Mathf.PI * 2;
        moveDir = new Vector2(Mathf.Cos(moveDirRad), Mathf.Sin(moveDirRad));
    }

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

    public override float Damage(float damage)
    {
        damage = Mathf.Min(damage, health);

        accumulatedDamage += damage;
        while (accumulatedDamage > damagePerShoot)
        {
            accumulatedDamage -= damagePerShoot;
            Shoot(Random.Range(minBulletsShot, maxBulletsShot + 1));
        }

        return base.Damage(damage);
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