using UnityEngine;

public class ShieldController : PhysicsObject
{
    public ShieldBulletAttractor bulletAttractor;

    public SpriteRenderer shieldRenderer;

    public SpriteRenderer[] wireRenderers;

    public CircularHealthBar healthBar;

    public ParticleSystem damageParticles;

    public ParticleSystem deactivateParticles;

    public float maxHealth;

    public float health;

    public Color shieldDeactivatedColor;

    public Color wireDeactivatedColor;

    private void Start()
    {
        physicsWorld.AddObject(this);

        healthBar.fillAmount = health / maxHealth;
    }

    public void Damage(float damage)
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
        }
    }

    public override void OnObjectCollision(PhysicsObject otherObj, Vector2 point, Vector2 normal)
    {
        if (otherObj is PlayerSpaceship && health > 0)
        {
            (otherObj as PlayerSpaceship).StunFromCollision(-normal);
        }
    }

    private void Deactivate()
    {
        bulletAttractor.Deactivate();

        shieldRenderer.color = shieldDeactivatedColor;
        foreach (SpriteRenderer sr in wireRenderers)
        {
            sr.color = wireDeactivatedColor;
        }
        deactivateParticles.Play();
    }
}