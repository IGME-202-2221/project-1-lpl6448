using UnityEngine;

/// <summary>
/// Abstract class representing an object that can be part of the physics simulation.
/// Collision methods are left unimplemented so that subclasses can provide custom physics behaviors.
/// Author: Luke Lepkowski (lpl6448@rit.edu)
/// </summary>
public abstract class PhysicsObject : MonoBehaviour
{
    /// <summary>
    /// PhysicsWorld that this object belongs to
    /// </summary>
    public PhysicsWorld physicsWorld;

    /// <summary>
    /// Integer collision layer, which is used effectively as an identifier for different PhysicsObjects
    /// </summary>
    public int collisionLayer;

    /// <summary>
    /// Local-space offset of the center of this object's collider circle
    /// </summary>
    public Vector2 circleOffset;

    /// <summary>
    /// World-space center of this object's collider circle
    /// </summary>
    public Vector2 worldCircleCenter => transform.TransformPoint(circleOffset);

    /// <summary>
    /// Local-space (unscaled) radius of this object's collider circle
    /// </summary>
    public float circleRadius = 0.5f;

    /// <summary>
    /// World-space (scaled) radius of this object's collider circle
    /// </summary>
    public float worldCircleRadius => circleRadius * Mathf.Max(transform.localScale.x, transform.localScale.y, transform.localScale.z);

    /// <summary>
    /// Current velocity (units/s) of this object
    /// </summary>
    [HideInInspector]
    public Vector2 velocity;

    /// <summary>
    /// Current angular velocity (degrees/s) of this object
    /// </summary>
    [HideInInspector]
    public float angularVelocity;

    /// <summary>
    /// Runs a basic physics tick for this object, moving and rotating the object according
    /// to velocity and angularVelocity. Other parts of the simulation, like collision and
    /// custom logic, are left to PhysicsObject subclasses and the PhysicsWorld.
    /// </summary>
    /// <param name="deltaTime">Time since the left physics tick (in seconds)</param>
    protected void PhysicsTick(float deltaTime)
    {
        transform.Translate(velocity * deltaTime, Space.World);
        transform.Rotate(0, 0, angularVelocity * deltaTime);
    }

    /// <summary>
    /// Called by the PhysicsWorld whenever this object collides with the map boundaries
    /// </summary>
    /// <param name="point">World-space point on this object's collider circle where a collision was detected</param>
    /// <param name="normal">World-space normal of the collision (out from the point on this object's collider circle)</param>
    public virtual void OnWallCollision(Vector2 point, Vector2 normal) { }

    /// <summary>
    /// Called by the PhysicsWorld whenever this object collides with another PhysicsObject
    /// </summary>
    /// <param name="otherObj">PhysicsObject that this object collided with</param>
    /// <param name="point">World-space point on this object's collider circle where a collision was detected</param>
    /// <param name="normal">World-space normal of the collision (out from the point on this object's collider circle)</param>
    public virtual void OnObjectCollision(PhysicsObject otherObj, Vector2 point, Vector2 normal) { }

    /// <summary>
    /// When this object is selected in the inspector, draws a wire sphere representing the current collider circle
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(worldCircleCenter, worldCircleRadius);
    }
}