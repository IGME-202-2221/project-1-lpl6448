using UnityEngine;

// Comments

/// <summary>
/// Superclass for all spaceships in the game, containing common information such as
/// health, stun information and effects, and damage effects.
/// 
/// NOTE: I'm planning to move many common behaviors from PlayerSpaceship and EnemySpaceship
/// into this class in the future, but right now it only contains health/stun/damage.
/// 
/// Author: Luke Lepkowski (lpl6448@rit.edu)
/// </summary>
public class Spaceship : PhysicsObject
{
    public GameManager gameManager;

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
    public bool Stun(float seconds)
    {
        if (!stunned && seconds > 0 && Time.time - stunEndTime >= stunCooldown)
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
    /// Damages this ship by the indicated amount of damage, playing the damageParticles and activating the damageFlash
    /// </summary>
    /// <param name="damage">Amount of damage that will be subtracted from this ship's health</param>
    public void Damage(float damage)
    {
        health -= damage;
        if (health <= 0)
        {
            health = 0;
            Death();
        }

        damageParticles.Play();
        damageFlash.SetActive(true);
        damageFlashEndTime = Time.time + damageFlashDuration;
    }

    protected virtual void Death() { }

    /// <summary>
    /// Attempts to stun this ship and applies the collision stun impulses
    /// </summary>
    /// <param name="normal">Normal direction of the collision, used to knock this ship back</param>
    public void StunFromCollision(Vector2 normal)
    {
        if (Stun(shipCollisionStunTime))
        {
            velocity = -normal * shipCollisionStunImpulse;
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