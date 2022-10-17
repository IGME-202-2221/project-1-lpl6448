using UnityEngine;
using System.Collections;

/// <summary>
/// Superclass for all spaceships in the game, containing common information such as
/// health, stun information and effects, and damage effects.
/// Author: Luke Lepkowski (lpl6448@rit.edu)
/// </summary>
public abstract class Spaceship : PhysicsObject
{
    /// <summary>
    /// Reference to the GameManager, used for awarding score
    /// </summary>
    public GameManager gameManager;

    /// <summary>
    /// Amount of score added for each health point of damage taken on this ship
    /// </summary>
    [Header("Score")]
    public int scorePerDamage;

    /// <summary>
    /// Amount of score added when this ship is destroyed
    /// </summary>
    public int scoreOnDeath;

    /// <summary>
    /// Initial and maximum health of this ship
    /// </summary>
    [Header("Health and Stun")]
    public float maxHealth = 100;

    /// <summary>
    /// Current health of this ship (0 - maxHealth)
    /// </summary>
    public float health;

    /// <summary>
    /// Whether this ship is "dead" (has 0 health) or not
    /// </summary>
    public bool dead => health <= 0;

    /// <summary>
    /// Prefab that is instantiated at the end of this ship's death animation
    /// </summary>
    public GameObject shipExplode;

    /// <summary>
    /// Whether this ship is affected by impulses (from bullets) or not
    /// </summary>
    public bool canHaveImpulse = true;

    /// <summary>
    /// Whether this ship can be stunned by collisions and bullets
    /// </summary>
    public bool canBeStunned = true;

    /// <summary>
    /// Minimum time (in seconds) that this ship is invulnerable to being stunned after the previous stun wears off
    /// </summary>
    public float stunCooldown;

    /// <summary>
    /// Reference to the GameObject that is activated or deactivated depending on whether the ship is stunned
    /// </summary>
    public GameObject stunFlash;

    /// <summary>
    /// Reference to the GameObject that is activated (and deactivated shortly after) when this ship takes damage
    /// </summary>
    public GameObject damageFlash;

    /// <summary>
    /// ParticleSystem that is played whenever this ship takes damage
    /// </summary>
    public ParticleSystem damageParticles;

    /// <summary>
    /// Time (in seconds) that the damageFlash remains active for when this ship takes damage
    /// </summary>
    public float damageFlashDuration;

    /// <summary>
    /// Time (in seconds) that this ship is stunned for after running into another ship
    /// </summary>
    public float shipCollisionStunTime;

    /// <summary>
    /// Knockback (units/s) given to thie ship whenever it collides with another ship
    /// </summary>
    public float shipCollisionStunImpulse;

    /// <summary>
    /// Maximum angular knockback (degrees/s) given to this ship. (Whenever it collides with another ship,
    /// its angular velocity is changed to a random number within this value.
    /// </summary>
    public float shipCollisionStunAngularImpulse;

    /// <summary>
    /// Whether this ship is currently stunned and therefore cannot move or shoot
    /// </summary>
    protected bool stunned;

    /// <summary>
    /// In-game time that this ship was stunned
    /// </summary>
    protected float stunStartTime;

    /// <summary>
    /// In-game time that this ship will be unstunned
    /// </summary>
    protected float stunEndTime;

    /// <summary>
    /// In-game time that the damageFlash will be deactivated
    /// </summary>
    protected float damageFlashEndTime;

    /// <summary>
    /// Attempts to stun this ship for the given number of seconds, disabling its movement and shooting.
    /// This function may return false if the ship is already stunned, is under a stun cooldown, or seconds == 0.
    /// </summary>
    /// <param name="seconds">Number of seconds to stun this ship for</param>
    /// <returns>Whether this ship was successfully stunned or not</returns>
    public virtual bool Stun(float seconds)
    {
        if (canBeStunned && !stunned && seconds > 0 && Time.time - stunEndTime >= stunCooldown)
        {
            stunned = true;
            stunStartTime = Time.time;
            stunEndTime = Time.time + seconds;
            stunFlash.SetActive(true);
            return true;
        }
        else
        {
            return false;
        }
    }

    /// <summary>
    /// Knocks this ship back by the impulse, requires canHaveImpulse to be true
    /// </summary>
    /// <param name="impulse">Vector2 added to the velocity if this ship is affected by impulses</param>
    public virtual void Impulse(Vector2 impulse)
    {
        if (canHaveImpulse)
        {
            velocity += impulse;
        }
    }

    /// <summary>
    /// Damages this ship by the indicated amount of damage, playing the damageParticles and activating the damageFlash
    /// </summary>
    /// <param name="damage">Amount of damage that will be subtracted from this ship's health</param>
    /// <param name="addScore">Whether score should be added for this damage</param>
    /// <returns>Amount of damage done to the ship</returns>
    public virtual float Damage(float damage, bool addScore)
    {
        float oldHealth = health;

        health -= damage;
        if (health <= 0)
        {
            health = 0;
            if (oldHealth > 0)
            {
                Death();
            }
        }

        if (addScore)
        {
            gameManager.AddScore(Mathf.FloorToInt(Mathf.Max(0, oldHealth - health) * scorePerDamage));
        }

        damageParticles.Play();
        damageFlash.SetActive(true);
        damageFlashEndTime = Time.time + damageFlashDuration;

        return oldHealth - health;
    }

    /// <summary>
    /// When this ship dies, begin the death animation
    /// </summary>
    private void Death()
    {
        StartCoroutine(DeathAnimation());
    }

    /// <summary>
    /// Runs a death animation that inflates and then shrinks the ship before finally
    /// instantiating the shipExplode effect, awarding score, and destroying this ship
    /// </summary>
    /// <returns>IEnumerator for the Unity coroutine</returns>
    private IEnumerator DeathAnimation()
    {
        Vector3 startScale = transform.localScale;
        float startTime = Time.time;
        float duration = 0.6f;
        while (Time.time - startTime < duration)
        {
            float t = (Time.time - startTime) / duration;
            // Adapted from https://easings.net/#easeInBack
            float st = 1 - (4 * t * t * t - 3 * t * t);

            transform.localScale = startScale * st;
            yield return null;
        }

        Instantiate(shipExplode, transform.position, Quaternion.identity);

        if (scoreOnDeath > 0)
        {
            gameManager.AddScoreWithVisualAddition(scoreOnDeath, transform.position);
        }

        Destroy(gameObject);
    }

    /// <summary>
    /// Attempts to stun this ship and applies the collision stun impulses
    /// </summary>
    /// <param name="normal">Normal direction of the collision, used to knock this ship back</param>
    public void StunFromCollision(Vector2 normal)
    {
        if (Stun(shipCollisionStunTime))
        {
            Impulse(-normal * shipCollisionStunImpulse);
            angularVelocity = Random.Range(-1f, 1f) * shipCollisionStunAngularImpulse;
        }
    }

    /// <summary>
    /// Called by subclasses in their Update functions to deactivate stunFlash and damageFlash when needed
    /// </summary>
    protected void UpdateFlashes()
    {
        if (stunned && Time.time >= stunEndTime)
        {
            stunned = false;
            stunFlash.SetActive(false);
        }
        if (damageFlash.activeSelf && Time.time >= damageFlashEndTime)
        {
            damageFlash.SetActive(false);
        }
    }
}