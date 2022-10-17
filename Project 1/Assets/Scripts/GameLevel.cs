using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Component containing data about one level of the game.
/// Author: Luke Lepkowski (lpl6448@rit.edu)
/// </summary>
public class GameLevel : MonoBehaviour
{
    /// <summary>
    /// Whether this level is the final "win" level. Once the win level is reached, the game ends
    /// and no enemies are spawned.
    /// </summary>
    public bool isWinLevel;

    /// <summary>
    /// Transform that the Camera is moved to while this level is active
    /// </summary>
    public Transform cameraCenter;

    /// <summary>
    /// Minimum bounds (local-space) of the map while this level is active
    /// </summary>
    public Vector2 boundsMin;

    /// <summary>
    /// Minimum bounds (world-space) of the map while this level is active
    /// </summary>
    public Vector2 worldBoundsMin => transform.TransformPoint(boundsMin);

    /// <summary>
    /// Maximum bounds (local-space) of the map while this level is active
    /// </summary>
    public Vector2 boundsMax;

    /// <summary>
    /// Maximum bounds (world-space) of the map while this level is active
    /// </summary>
    public Vector2 worldBoundsMax => transform.TransformPoint(boundsMax);

    /// <summary>
    /// List of ShieldControllers that are inside of this level. All of them must be deactivated
    /// before the level is declared finished.
    /// </summary>
    public List<ShieldController> shieldControllers;

    /// <summary>
    /// Reference to the ShieldLaser for this level. After the level is completed, the laser will
    /// deactivate, allowing the player to go through to the next level.
    /// </summary>
    public ShieldLaser shieldLaser;

    /// <summary>
    /// Reference to the left door that spawns enemies during the level
    /// </summary>
    public SpawnDoor spawnDoorLeft;

    /// <summary>
    /// Reference to the right door that spawns enemies during the level
    /// </summary>
    public SpawnDoor spawnDoorRight;

    /// <summary>
    /// List of LevelWaves that this level contains
    /// </summary>
    public List<LevelWave> waves;

    /// <summary>
    /// When selected, draw a wire cube to illustrate this level's boundaries
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube((worldBoundsMin + worldBoundsMax) / 2, worldBoundsMax - worldBoundsMin);
    }
}