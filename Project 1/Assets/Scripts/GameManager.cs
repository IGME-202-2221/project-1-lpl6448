using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public PhysicsWorld physicsWorld;

    public List<ShieldController> shieldControllers;

    public ShieldLaser shieldLaser;

    public SpawnDoor spawnDoorLeft;

    public SpawnDoor spawnDoorRight;

    public Transform enemyTarget;

    public float enemySpawnChancePerSecond;

    public float enemySpawnCooldown;

    public List<LevelWave> waves;

    [HideInInspector]
    public List<EnemySpaceship> enemiesInWave = new List<EnemySpaceship>();

    private Queue<EnemySpaceship> enemiesToSpawn = new Queue<EnemySpaceship>();

    private void Start()
    {
        StartCoroutine(DoLevel());
    }

    private IEnumerator DoLevel()
    {
        yield return new WaitForSeconds(1);

        for (int i = 0; i < waves.Count; i++)
        {
            print("Beginning wave " + i);
            LevelWave wave = waves[i];

            // Add this wave's enemies to the queue
            foreach (EnemySpaceship enemyPrefab in wave.enemyPrefabs)
            {
                enemiesToSpawn.Enqueue(enemyPrefab);
            }

            // As long as the player hasn't progressed past this wave, spawn more enemies and wait
            float lastEnemySpawn = -1;
            while (i == GetWave())
            {
                float enemySpawnChancePerFrame = 1 - Mathf.Pow(1 - enemySpawnChancePerSecond, Time.deltaTime);
                if (Time.time - lastEnemySpawn >= enemySpawnCooldown && (Random.value < enemySpawnChancePerFrame || enemiesInWave.Count == 0))
                {
                    SpawnEnemiesFromQueue(wave.maxEnemiesOnMap);
                    lastEnemySpawn = Time.time;
                }
                yield return null;
            }

            // At the end of a wave, the enemies in the wave stay on the map but are cleared from the list.
            // This will mean that, if a player tries to knock out shields before knocking out enemies,
            // the game will spawn more enemies to increase difficulty.
            enemiesInWave.Clear();

            // Once the wave is finished, the unspawned enemies should not carry over to future waves
            enemiesToSpawn.Clear();
        }

        // After a short wait, deactivate the shield laser
        yield return new WaitForSeconds(1.5f);
        shieldLaser.Deactivate();
    }

    private void SpawnEnemiesFromQueue(int maxEnemiesOnMap)
    {
        int enemyCountToSpawn = Mathf.Min(enemiesToSpawn.Count, maxEnemiesOnMap - enemiesInWave.Count);

        // Figure out which enemies spawn where using round-robin
        List<EnemySpaceship> enemiesToSpawnLeft = new List<EnemySpaceship>();
        List<EnemySpaceship> enemiesToSpawnRight = new List<EnemySpaceship>();
        int leftDoorIndex = Random.Range(0, 2);
        for (int i = 0; i < enemyCountToSpawn; i++)
        {
            EnemySpaceship enemyToSpawn = enemiesToSpawn.Dequeue();
            if (i % 2 == leftDoorIndex)
            {
                enemiesToSpawnLeft.Add(enemyToSpawn);
            }
            else
            {
                enemiesToSpawnRight.Add(enemyToSpawn);
            }
        }

        // Spawn enemies
        if (enemiesToSpawnLeft.Count > 0)
        {
            spawnDoorLeft.SpawnEnemies(enemiesToSpawnLeft, this);
        }
        if (enemiesToSpawnRight.Count > 0)
        {
            spawnDoorRight.SpawnEnemies(enemiesToSpawnRight, this);
        }
    }

    private int GetWave()
    {
        float totalHealth = 0;
        float totalMaxHealth = 0;
        foreach (ShieldController shieldController in shieldControllers)
        {
            totalHealth += shieldController.health;
            totalMaxHealth += shieldController.maxHealth;
        }
        float percentCompletion = 1 - totalHealth / totalMaxHealth;

        return Mathf.FloorToInt(percentCompletion * waves.Count);
    }
}