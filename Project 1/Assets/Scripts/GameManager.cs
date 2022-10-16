using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public PhysicsWorld physicsWorld;

    public Transform cameraTransform;

    public PlayerSpaceship playerShip;

    public float enemySpawnChancePerSecond;

    public float enemySpawnCooldown;

    public List<GameLevel> levels;

    public UILevelPanel levelPanel;

    public ScoreAddition scoreAdditionPrefab;

    public UIScoreDisplay scoreDisplay;

    public int score;

    [HideInInspector]
    public List<EnemySpaceship> enemiesInWave = new List<EnemySpaceship>();

    private List<EnemySpaceship> activeEnemies = new List<EnemySpaceship>();

    private Queue<EnemySpaceship> enemiesToSpawn = new Queue<EnemySpaceship>();

    private GameLevel activeLevel;

    public void AddEnemy(EnemySpaceship enemy)
    {
        activeEnemies.Add(enemy);
        enemiesInWave.Add(enemy);
    }
    public void RemoveEnemy(EnemySpaceship enemy)
    {
        activeEnemies.Remove(enemy);
        enemiesInWave.Remove(enemy);
    }

    public void AddScore(int addedScore)
    {
        score += addedScore;
        scoreDisplay.score = score;
    }

    public void AddScoreWithVisualAddition(int addedScore, Vector2 scorePos)
    {
        AddScore(addedScore);

        ScoreAddition addition = Instantiate(scoreAdditionPrefab, scorePos, Quaternion.identity);
        addition.SetScore(addedScore);
    }

    private void Start()
    {
        StartCoroutine(DoGame());
    }

    public bool go;
    private void Update()
    {
        if (go)
        {
            go = false;
            foreach (ShieldController c in activeLevel.shieldControllers)
            {
                c.Damage(10000);
            }
        }
    }

    private IEnumerator DoGame()
    {
        levelPanel.SetActiveLevel(0);

        for (int i = 0; i < levels.Count; i++)
        {
            GameLevel level = levels[i];
            if (level.isWinLevel)
            {
                break;
            }

            print("Beginning level " + i);
            yield return DoLevel(level);

            // Update the level panel with this level's completion
            levelPanel.SetActiveLevel(i + 1);

            yield return new WaitForSeconds(3);

            // Wait for the player to go toward the new level if possible
            float waitStart = Time.time;
            while (Time.time - waitStart < 4 && level.worldBoundsMax.y - playerShip.transform.position.y > 5)
            {
                yield return null;
            }

            yield return DoLevelTransition(level, i < levels.Count - 1 ? levels[i + 1] : null);
        }

        print("GAME WIN");
    }

    private IEnumerator DoLevelTransition(GameLevel prevLevel, GameLevel newLevel)
    {
        // If there is a new level, move the camera and map boundaries to the next level
        if (newLevel != null)
        {
            physicsWorld.mapBoundsMax = newLevel.worldBoundsMax;

            float camZ = cameraTransform.position.z;
            float startTime = Time.time;
            float duration = 3;
            while (Time.time - startTime < duration)
            {
                float t = (Time.time - startTime) / duration;
                float st = Mathf.SmoothStep(0, 1, t);

                Vector2 camPos = Vector2.Lerp(prevLevel.cameraCenter.position, newLevel.cameraCenter.position, st);
                cameraTransform.position = new Vector3(camPos.x, camPos.y, camZ);
                physicsWorld.mapBoundsMin = Vector2.Lerp(prevLevel.worldBoundsMin, newLevel.worldBoundsMin, st);
                yield return null;
            }

            transform.position = newLevel.cameraCenter.position;
            physicsWorld.mapBoundsMin = newLevel.worldBoundsMin;
        }
    }

    private IEnumerator DoLevel(GameLevel level)
    {
        activeLevel = level;
        physicsWorld.mapBoundsMin = level.worldBoundsMin;
        physicsWorld.mapBoundsMax = level.worldBoundsMax;

        yield return new WaitForSeconds(1);

        for (int i = 0; i < activeLevel.waves.Count; i++)
        {
            print("Beginning wave " + i);
            LevelWave wave = activeLevel.waves[i];

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
        activeLevel.shieldLaser.Deactivate();

        // Destroy any remaining enemies
        foreach (EnemySpaceship enemy in activeEnemies)
        {
            enemy.Damage(10000);
        }

        // Regen the player
        playerShip.health = playerShip.maxHealth;
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
            activeLevel.spawnDoorLeft.SpawnEnemies(enemiesToSpawnLeft, this);
        }
        if (enemiesToSpawnRight.Count > 0)
        {
            activeLevel.spawnDoorRight.SpawnEnemies(enemiesToSpawnRight, this);
        }
    }

    private int GetWave()
    {
        float totalHealth = 0;
        float totalMaxHealth = 0;
        foreach (ShieldController shieldController in activeLevel.shieldControllers)
        {
            totalHealth += shieldController.health;
            totalMaxHealth += shieldController.maxHealth;
        }
        float percentCompletion = 1 - totalHealth / totalMaxHealth;

        return Mathf.FloorToInt(percentCompletion * activeLevel.waves.Count);
    }
}