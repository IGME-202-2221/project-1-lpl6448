using UnityEngine;

/// <summary>
/// Type of PhysicsObject that represents any bullet shot by a Spaceship.
/// Author: Luke Lepkowski (lpl6448@rit.edu)
/// </summary>
public class Bullet : PhysicsObject
{
    /// <summary>
    /// Damage done to whatever Spaceship this bullet hits
    /// </summary>
    public float hitDamage;

    /// <summary>
    /// Duration of time (in seconds) to stun whatever Spaceship this bullet hits
    /// </summary>
    public float hitStunTime;

    /// <summary>
    /// Impulse or knockback (units/s) to whatever Spaceship this bullet hits
    /// </summary>
    public float hitImpulse;

    /// <summary>
    /// Speed (units/s) of this bullet
    /// </summary>
    public float speed;

    /// <summary>
    /// Reference to the prefab to instantiate when this bullet collides with a wall or another bullet
    /// (particle system to create an explosion effect)
    /// </summary>
    public GameObject bulletExplode;

    /// <summary>
    /// Reference to the Spaceship that shot this bullet
    /// </summary>
    [HideInInspector]
    public Spaceship Origin;

    /// <summary>
    /// When this bullet is created, sets the initial velocity to this transform's up, with a given speed
    /// </summary>
    private void Start()
    {
        velocity = transform.up * speed;
    }

    /// <summary>
    /// Every frame, runs a physics tick to update the position of the bullet
    /// </summary>
    private void Update()
    {
        PhysicsTick(Time.deltaTime);
    }

    /// <summary>
    /// When this bullet is destroyed, removes it from the physics simulation
    /// </summary>
    private void OnDestroy()
    {
        physicsWorld.RemoveObject(this);
    }

    /// <summary>
    /// Called by the PhysicsWorld whenever this object collides with the map boundaries, causing the bullet to explode
    /// </summary>
    /// <param name="point">World-space point on this object's collider circle where a collision was detected</param>
    /// <param name="normal">World-space normal of the collision (out from the point on this object's collider circle)</param>
    public override void OnWallCollision(Vector2 point, Vector2 normal)
    {
        Explode(point);
    }

    /// <summary>
    /// Called by the PhysicsWorld whenever this object collides with another PhysicsObject.
    /// If the bullet hits a Spaceship (not the one that shot it), it causes damage, causes knockback, and stuns the Spaceship.
    /// </summary>
    /// <param name="otherObj">PhysicsObject that this object collided with</param>
    /// <param name="point">World-space point on this object's collider circle where a collision was detected</param>
    /// <param name="normal">World-space normal of the collision (out from the point on this object's collider circle)</param>
    public override void OnObjectCollision(PhysicsObject otherObj, Vector2 point, Vector2 normal)
    {
        // Collision layers are used here to define which spaceships each bullet can hit
        // (set in the Inspector for each Bullet and Spaceship prefab)
        if (otherObj.collisionLayer == collisionLayer)
        {
            if (otherObj is Spaceship && otherObj != Origin)
            {
                Spaceship spaceship = otherObj as Spaceship;
                spaceship.Damage(hitDamage, true);
                spaceship.Stun(hitStunTime);
                spaceship.Impulse(velocity.normalized * hitImpulse);

                Destroy(gameObject);
            }
            else if (otherObj is ShieldController)
            {
                ShieldController shieldController = otherObj as ShieldController;
                float damage = shieldController.Damage(hitDamage);

                if (damage > 0)
                {
                    Destroy(gameObject);
                }
            }
            else if (!(otherObj is Bullet))
            {
                Explode(point);
            }
        }
        else if (otherObj is Bullet && (otherObj as Bullet).Origin != Origin)
        {
            Explode(point);
        }
    }

    /// <summary>
    /// Destroys this bullet and causes a lingering explosion effect
    /// </summary>
    /// <param name="point">World-space point to instantiate bulletExplode at</param>
    private void Explode(Vector2 point)
    {
        // Instantiate bullet explosion effect
        Instantiate(bulletExplode, point, Quaternion.identity);

        // Destroy this bullet
        Destroy(gameObject);
    }
}