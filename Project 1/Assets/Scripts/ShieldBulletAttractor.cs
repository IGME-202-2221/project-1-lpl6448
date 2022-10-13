using UnityEngine;

public class ShieldBulletAttractor : PhysicsObject
{
    public float attractorForce;

    public int targetCollisionLayer;

    public SpriteRenderer radiusRenderer;

    private void Start()
    {
        physicsWorld.AddObject(this);
    }
    private void OnDestroy()
    {
        physicsWorld.RemoveObject(this);
    }

    public void Deactivate()
    {
        radiusRenderer.enabled = false;
        enabled = false;
    }

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