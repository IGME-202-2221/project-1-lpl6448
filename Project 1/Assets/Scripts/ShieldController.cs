using UnityEngine;

public class ShieldController : PhysicsObject
{
    public GameManager gameManager;

    public ShieldBulletAttractor bulletAttractor;

    public SpriteRenderer shieldRenderer;

    public SpriteRenderer[] wireRenderers;

    public CircularHealthBar healthBar;

    public ParticleSystem damageParticles;

    public ParticleSystem deactivateParticles;

    public int scorePerDamage;

    public int scoreOnDestroy;

    public float maxHealth;

    public float health;

    public Color shieldDeactivatedColor;

    public Color wireDeactivatedColor;

    private void Start()
    {
        physicsWorld.AddObject(this);

        healthBar.fillAmount = health / maxHealth;
    }

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

    public override void OnObjectCollision(PhysicsObject otherObj, Vector2 point, Vector2 normal)
    {
        if (otherObj is Spaceship && health > 0)
        {
            (otherObj as Spaceship).StunFromCollision(-normal);
        }
    }

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