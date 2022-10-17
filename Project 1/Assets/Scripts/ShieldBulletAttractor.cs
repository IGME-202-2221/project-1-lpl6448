using UnityEngine;

/// <summary>
/// Represents a region around a shield station/ShieldController that attracts bullets
/// toward itself using the inverse-square law.
/// Author: Luke Lepkowski (lpl6448@rit.edu)
/// </summary>
public class ShieldBulletAttractor : PhysicsObject
{
    /// <summary>
    /// Force applied to nearby bullets (within the collision circle)
    /// </summary>
    public float attractorForce;

    /// <summary>
    /// Collision layer of affected bullets (so that only player bullets are affected)
    /// </summary>
    public int targetCollisionLayer;

    /// <summary>
    /// SpriteRenderer of a representation of the attraction area, deactivated
    /// when the ShieldController is deactivated
    /// </summary>
    public SpriteRenderer radiusRenderer;

    /// <summary>
    /// When created, add this attractor to the collision world
    /// </summary>
    private void Start()
    {
        physicsWorld.AddObject(this);
    }
    
    /// <summary>
    /// When destroyed, remove this attractor from the collision world
    /// </summary>
    private void OnDestroy()
    {
        physicsWorld.RemoveObject(this);
    }

    /// <summary>
    /// Called by the ShieldController to disable bullet attraction and disable the radiusRenderer
    /// </summary>
    public void Deactivate()
    {
        radiusRenderer.enabled = false;
        enabled = false;
    }

    /// <summary>
    /// Called by the PhysicsWorld when a collision is detected. If colliding with a bullet on the
    /// targetCollisionLayer, use the inverse-square law to accelerate the bullet toward the center
    /// </summary>
    /// <param name="otherObj">PhysicsObject that this object collided with</param>
    /// <param name="point">World-space point of the collision</param>
    /// <param name="normal">World-space normal of the collision</param>
    public override void OnObjectCollision(PhysicsObject otherObj, Vector2 point, Vector2 normal)
    {
        if (enabled && otherObj.collisionLayer == targetCollisionLayer && otherObj is Bullet)
        {
            float disSqr = ((Vector2)(transform.position - otherObj.transform.position)).sqrMagnitude;

            float bulletSpeed = otherObj.velocity.magnitude;
            Vector2 bulletDir = otherObj.velocity / bulletSpeed;

            Vector2 newBulletDir = (bulletDir - normal * attractorForce / disSqr * Time.deltaTime).normalized;
            Vector2 newBulletVelocity = newBulletDir * bulletSpeed;
            otherObj.velocity = newBulletVelocity;
            otherObj.transform.up = newBulletDir;
        }
    }
}