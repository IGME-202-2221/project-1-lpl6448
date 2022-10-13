using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SpawnDoor : MonoBehaviour
{
    public Transform shipSpawnLocation;

    public float shipSpawnRotationDifference;

    public float shipSpawnVelocityMin;

    public float shipSpawnVelocityMax;

    public float shipSpawnStunSeconds;

    public float shipSpawnCooldown;

    public float openSeconds;

    public float holdSeconds;

    public float closeSeconds;

    public Vector3 openOffset;

    private Vector3 closedPos;

    private void Start()
    {
        closedPos = transform.localPosition;
    }

    public void SpawnEnemies(List<EnemySpaceship> enemyPrefabs, GameManager gameManager)
    {
        StartCoroutine(AnimateSpawnEnemies(enemyPrefabs, gameManager));
    }

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
            enemy.target = gameManager.enemyTarget;
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

        // Add the ships to the collision simulation and close the door
        foreach (EnemySpaceship enemy in enemies)
        {
            //world.AddObject(enemy);
        }
        yield return AnimatePosition(false);
    }

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