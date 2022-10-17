using UnityEngine;

/// <summary>
/// Represents a "shield station" that has its own health and must be deactivated in order
/// to progress through the game.
/// Author: Luke Lepkowski (lpl6448@rit.edu)
/// </summary>
public class ShieldController : PhysicsObject
{
    /// <summary>
    /// Reference to the GameManager, for awarding score
    /// </summary>
    public GameManager gameManager;

    /// <summary>
    /// Reference to the ShieldBulletAttractor that is deactivated when this ShieldController is deactivated
    /// </summary>
    public ShieldBulletAttractor bulletAttractor;

    /// <summary>
    /// SpriteRenderer for the shield station that changes color when deactivated
    /// </summary>
    public SpriteRenderer shieldRenderer;

    /// <summary>
    /// Array of SpriteRenderers for the wires that change color when deactivated
    /// </summary>
    public SpriteRenderer[] wireRenderers;

    /// <summary>
    /// Reference to the CircularHealthBar used for the ShieldController
    /// </summary>
    public CircularHealthBar healthBar;

    /// <summary>
    /// ParticleSystem that is played when this ShieldController takes damage
    /// </summary>
    public ParticleSystem damageParticles;

    /// <summary>
    /// ParticleSystem that is played when this ShieldController is deactivated
    /// </summary>
    public ParticleSystem deactivateParticles;

    /// <summary>
    /// Amount of score to add for each health point of damage that the ShieldController takes
    /// </summary>
    public int scorePerDamage;

    /// <summary>
    /// Amount of score to add when this ShieldController is deactivated
    /// </summary>
    public int scoreOnDestroy;

    /// <summary>
    /// Maximum/starting health of this ShieldController
    /// </summary>
    public float maxHealth;

    /// <summary>
    /// Current health of this ShieldController, deactivated when it reaches 0
    /// </summary>
    public float health;

    /// <summary>
    /// Color that shieldRenderer changes to when this ShieldController is deactivated
    /// </summary>
    public Color shieldDeactivatedColor;

    /// <summary>
    /// Color that all of the wireRenderers change to when this ShieldController is deactivated
    /// </summary>
    public Color wireDeactivatedColor;

    /// <summary>
    /// To initialize, add this ShieldController to the physics simulation and initialize
    /// the health bar
    /// </summary>
    private void Start()
    {
        physicsWorld.AddObject(this);

        healthBar.fillAmount = health / maxHealth;
    }

    /// <summary>
    /// Damages this ShieldController by the given amount, updating the health bar and
    /// deactivating the shield station if necessary
    /// </summary>
    /// <param name="damage">Amount of damage dealt</param>
    /// <returns>The amount of damage taken by the ShieldController (less than damage if it was deactivated)</returns>
    public float Damage(float damage)
    {
        float oldHealth = health;

        if (oldHealth > 0)
        {
            health -= damage;
            if (health <= 0)
            {
                health = 0;
                Deactivate();
            }

            if (health < oldHealth)
            {
                healthBar.fillAmount = health / maxHealth;
                damageParticles.Play();
            }

            gameManager.AddScore(Mathf.FloorToInt(Mathf.Max(0, oldHealth - health) * scorePerDamage));
        }

        return oldHealth - health;
    }

    /// <summary>
    /// Called by the PhysicsWorld when a collision is detected and stuns any spaceship from
    /// a collision with the shield station
    /// </summary>
    /// <param name="otherObj">PhysicsObject that this object collided with</param>
    /// <param name="point">World-space point of the collision</param>
    /// <param name="normal">World-space normal of the collision</param>
    public override void OnObjectCollision(PhysicsObject otherObj, Vector2 point, Vector2 normal)
    {
        if (otherObj is Spaceship && health > 0)
        {
            (otherObj as Spaceship).StunFromCollision(-normal);
        }
    }

    /// <summary>
    /// Deactivates thie ShieldController, awarding score, deactivating the bullet attractor,
    /// and changing the colors of the wires and shield renderer
    /// </summary>
    private void Deactivate()
    {
        gameManager.AddScoreWithVisualAddition(scoreOnDestroy, transform.position);

        bulletAttractor.Deactivate();

        shieldRenderer.color = shieldDeactivatedColor;
        foreach (SpriteRenderer sr in wireRenderers)
        {
            sr.color = wireDeactivatedColor;
        }
        deactivateParticles.Play();
    }
}