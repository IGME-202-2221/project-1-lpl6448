using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Represents the spawn location for enemies in a level. This script also controls
/// the opening/closing of the door when these enemies are spawned.
/// Author: Luke Lepkowski (lpl6448@rit.edu)
/// </summary>
public class SpawnDoor : MonoBehaviour
{
    /// <summary>
    /// Transform containing the position that enemy ships are spawned at
    /// </summary>
    public Transform shipSpawnLocation;

    /// <summary>
    /// Maximum degrees of difference that ships are launched at (compared to the upward direction of shipSpawnLocation)
    /// </summary>
    public float shipSpawnRotationDifference;

    /// <summary>
    /// Minimum speed that ships are spawned with, in the upward direction of shipSpawnLocation
    /// </summary>
    public float shipSpawnVelocityMin;

    /// <summary>
    /// Maximum speed that ships are spawned with, in the upward direction of shipSpawnLocation
    /// </summary>
    public float shipSpawnVelocityMax;

    /// <summary>
    /// Number of seconds that ships are stunned for when spawned
    /// </summary>
    public float shipSpawnStunSeconds;

    /// <summary>
    /// Number of seconds between individual enemies spawning in this door
    /// </summary>
    public float shipSpawnCooldown;

    /// <summary>
    /// Duration (seconds) of the door open animation
    /// </summary>
    public float openSeconds;

    /// <summary>
    /// Number of seconds that this door stays open for when spawning enemies
    /// </summary>
    public float holdSeconds;

    /// <summary>
    /// Duration (seconds) of the door close animation
    /// </summary>
    public float closeSeconds;

    /// <summary>
    /// Local-space offset applied to this door when opened
    /// </summary>
    public Vector3 openOffset;

    /// <summary>
    /// Internal storage for the local-space closed position of this door
    /// </summary>
    private Vector3 closedPos;

    /// <summary>
    /// On creation, initialize closedPos to the local position
    /// </summary>
    private void Start()
    {
        closedPos = transform.localPosition;
    }

    /// <summary>
    /// Opens the door, spawns each of the enemies in enemyPrefabs, and closes the door
    /// </summary>
    /// <param name="enemyPrefabs">List of EnemySpaceship prefabs instantiated in this door before closing</param>
    /// <param name="gameManager">GameManager that called this action</param>
    public void SpawnEnemies(List<EnemySpaceship> enemyPrefabs, GameManager gameManager)
    {
        StartCoroutine(AnimateSpawnEnemies(enemyPrefabs, gameManager));
    }

    /// <summary>
    /// Opens the door, spawns each of the enemies in enemyPrefabs, and closes the door
    /// </summary>
    /// <param name="enemyPrefabs">List of EnemySpaceship prefabs instantiated in this door before closing</param>
    /// <param name="gameManager">GameManager that called this action</param>
    /// <returns>IEnumerator for the Unity coroutine</returns>
    private IEnumerator AnimateSpawnEnemies(List<EnemySpaceship> enemyPrefabs, GameManager gameManager)
    {
        // Open door
        yield return AnimatePosition(true);

        List<EnemySpaceship> enemies = new List<EnemySpaceship>();
        for (int i = 0; i < enemyPrefabs.Count; i++)
        {
            EnemySpaceship enemyPrefab = enemyPrefabs[i];

            // Spawn, stun, and launch enemy into the map.
            // This ship is not added to the collision world until it has had enough time to enter the map.
            Quaternion spawnRot = shipSpawnLocation.rotation * Quaternion.Euler(0, 0, Random.Range(-shipSpawnRotationDifference, shipSpawnRotationDifference));
            EnemySpaceship enemy = Instantiate(enemyPrefab, shipSpawnLocation.position, spawnRot);
            enemy.Stun(shipSpawnStunSeconds);
            enemy.velocity = enemy.transform.up * Random.Range(shipSpawnVelocityMin, shipSpawnVelocityMax);
            enemy.target = gameManager.playerShip != null ? gameManager.playerShip.transform : null;
            enemy.gameManager = gameManager;
            enemies.Add(enemy);

            // If there are more ships to spawn, wait to spawn the next one
            if (i < enemyPrefabs.Count - 1)
            {
                yield return new WaitForSeconds(shipSpawnCooldown);
            }
        }

        // Wait for last ship to go through door before closing
        yield return new WaitForSeconds(holdSeconds);

        // Close the door
        yield return AnimatePosition(false);
    }

    /// <summary>
    /// Animates the position of this door from open to closed or vice-versa
    /// </summary>
    /// <param name="open">Whether this door is animating to be open</param>
    /// <returns>IEnumerator for the Unity coroutine</returns>
    private IEnumerator AnimatePosition(bool open)
    {
        Vector3 startOffset = open ? Vector3.zero : openOffset;
        Vector3 endOffset = open ? openOffset : Vector3.zero;
        float duration = open ? openSeconds : closeSeconds;

        float startTime = Time.time;
        while (Time.time - startTime < duration)
        {
            float t = (Time.time - startTime) / duration;
            float st = 1 - (1 - t) * (1 - t);
            transform.localPosition = closedPos + Vector3.Lerp(startOffset, endOffset, st);
            yield return null;
        }

        transform.localPosition = closedPos + endOffset;
    }
}