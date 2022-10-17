using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Contains most of the game state and logic and keeps track of things such as the current level,
/// number of enemies, player ship, and score.
/// Author: Luke Lepkowski (lpl6448@rit.edu)
/// </summary>
public class GameManager : MonoBehaviour
{
    /// <summary>
    /// Reference to the game's PhysicsWorld
    /// </summary>
    public PhysicsWorld physicsWorld;

    /// <summary>
    /// Reference to the Transform of the Camera, which is animated between levels
    /// </summary>
    public Transform cameraTransform;

    /// <summary>
    /// Reference to the player's spaceship, or null if the player has lost
    /// </summary>
    public PlayerSpaceship playerShip;

    /// <summary>
    /// Probability (0-1) per second that more enemies will spawn during a level
    /// </summary>
    public float enemySpawnChancePerSecond;

    /// <summary>
    /// Minimum time (seconds) that must pass after a previous enemy spawn before more enemies come
    /// </summary>
    public float enemySpawnCooldown;

    /// <summary>
    /// List of GameLevels that are in the game (including the last "win" level)
    /// </summary>
    public List<GameLevel> levels;

    /// <summary>
    /// Reference to the level panel, which shows the player which level they are on
    /// </summary>
    public UILevelPanel levelPanel;
    
    /// <summary>
    /// Prefab that is instantiated when large amounts of score are acquired (such as after killing an enemy)
    /// </summary>
    public ScoreAddition scoreAdditionPrefab;

    /// <summary>
    /// Reference to the score display, which shows the player what their score is
    /// </summary>
    public UIScoreDisplay scoreDisplay;

    /// <summary>
    /// Current score (integer) of the game
    /// </summary>
    public int score;

    /// <summary>
    /// Reference to the Game Over panel that moves on screen once the player loses or wins
    /// </summary>
    public UIGameOver gameOverPanel;

    /// <summary>
    /// List of all enemies spawned in the current wave
    /// </summary>
    private List<EnemySpaceship> enemiesInWave = new List<EnemySpaceship>();

    /// <summary>
    /// List of all active enemies (from this wave and the previous waves)
    /// </summary>
    private List<EnemySpaceship> activeEnemies = new List<EnemySpaceship>();

    /// <summary>
    /// Queue of enemies that have not been spawned yet in this wave
    /// </summary>
    private Queue<EnemySpaceship> enemiesToSpawn = new Queue<EnemySpaceship>();

    /// <summary>
    /// Index of the active level (in the levels list)
    /// </summary>
    private int activeLevelIndex;

    /// <summary>
    /// Reference to the currently active level
    /// </summary>
    private GameLevel activeLevel;

    /// <summary>
    /// Whether the game is currently in progress (false if the game has ended)
    /// </summary>
    private bool gameInProgress;

    /// <summary>
    /// Registers an enemy as part of the game, adding it to the active enemies and enemiesInWave
    /// </summary>
    /// <param name="enemy">EnemySpaceship to add to the game</param>
    public void AddEnemy(EnemySpaceship enemy)
    {
        activeEnemies.Add(enemy);
        enemiesInWave.Add(enemy);
    }

    /// <summary>
    /// Removes an enemy from the active enemies and enemiesInWave, called when the enemy is destroyed
    /// </summary>
    /// <param name="enemy">Enemy to remove from the game</param>
    public void RemoveEnemy(EnemySpaceship enemy)
    {
        activeEnemies.Remove(enemy);
        enemiesInWave.Remove(enemy);
    }

    /// <summary>
    /// Adds the indicated amount of score to the game, updating the scoreDisplay
    /// </summary>
    /// <param name="addedScore">Integer representing the amount of score to be added</param>
    public void AddScore(int addedScore)
    {
        score += addedScore;
        scoreDisplay.score = score;
    }

    /// <summary>
    /// Adds the indicated amount of score to the game, updating the scoreDisplay
    /// and instantiating a ScoreAddition effect at scorePos
    /// </summary>
    /// <param name="addedScore">Integer representing the amount of score to be added</param>
    /// <param name="scorePos">World position that the ScoreAddition effect is instantiated at</param>
    public void AddScoreWithVisualAddition(int addedScore, Vector2 scorePos)
    {
        AddScore(addedScore);

        ScoreAddition addition = Instantiate(scoreAdditionPrefab, scorePos, Quaternion.identity);
        addition.SetScore(addedScore);
    }

    /// <summary>
    /// When the game starts, begin the game coroutine
    /// </summary>
    private void Start()
    {
        StartCoroutine(DoGame());
    }

    /// <summary>
    /// Every frame, check if the player has lost (if the playerShip has been destroyed).
    /// If so, animate the Game Over panel in.
    /// </summary>
    private void Update()
    {
        if (gameInProgress && playerShip == null)
        {
            gameOverPanel.In(score, activeLevelIndex, false);
            gameInProgress = false;
        }
    }

    /// <summary>
    /// Loads and iterates through the levels as the game progresses
    /// </summary>
    /// <returns>IEnumerator for the Unity coroutine</returns>
    private IEnumerator DoGame()
    {
        activeLevelIndex = 0;
        levelPanel.SetActiveLevel(0);
        gameInProgress = true;

        for (int i = 0; i < levels.Count; i++)
        {
            GameLevel level = levels[i];
            if (level.isWinLevel)
            {
                break;
            }

            print("Beginning level " + i);
            yield return DoLevel(level);
            activeLevelIndex++;

            // Update the level panel with this level's completion
            levelPanel.SetActiveLevel(i + 1);

            yield return new WaitForSeconds(3);

            // Wait for the player to go toward the new level if possible
            float waitStart = Time.time;
            while (playerShip == null || Time.time - waitStart < 4 && level.worldBoundsMax.y - playerShip.transform.position.y > 5)
            {
                yield return null;
            }

            yield return DoLevelTransition(level, i < levels.Count - 1 ? levels[i + 1] : null);
        }

        // Win condition
        yield return new WaitForSeconds(2);
        gameOverPanel.In(score, levels.Count - 1, true);
    }

    /// <summary>
    /// Animates a level transition between prevLevel and newLevel, moving both the Camera
    /// and map boundaries smoothly
    /// </summary>
    /// <param name="prevLevel">Previous GameLevel that the camera and boundaries are animated from</param>
    /// <param name="newLevel">Next GameLevel that the camera and boundaries are animated to</param>
    /// <returns>IEnumerator for the Unity coroutine</returns>
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

    /// <summary>
    /// Spawns enemies and keeps track of waves, finally ending the level when both ShieldControllers are deactivated
    /// </summary>
    /// <param name="level">Current GameLevel (which is set by this function to activeLevel)</param>
    /// <returns>IEnumerator for the Unity coroutine</returns>
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
        if (playerShip != null)
        {
            playerShip.health = playerShip.maxHealth;
        }
    }

    /// <summary>
    /// Spawns enemies from the queue until either the queue is empty or there are too many enemies on the map.
    /// Enemies alternate spawning between the left and right spawn doors.
    /// </summary>
    /// <param name="maxEnemiesOnMap">Maximum number of enemies that can be on the map at once (from this wave, not including previous waves)</param>
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

    /// <summary>
    /// Calculates the current wave index from the percentage of health left in both ShieldControllers
    /// </summary>
    /// <returns>The current wave index, floor(percent of health gone in ShieldControllers * wave count for this level)</returns>
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