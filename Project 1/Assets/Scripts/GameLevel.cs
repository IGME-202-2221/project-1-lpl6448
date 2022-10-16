using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameLevel : MonoBehaviour
{
    public bool isWinLevel;

    public Transform cameraCenter;

    public Vector2 boundsMin;

    public Vector2 worldBoundsMin => transform.TransformPoint(boundsMin);

    public Vector2 boundsMax;

    public Vector2 worldBoundsMax => transform.TransformPoint(boundsMax);

    public List<ShieldController> shieldControllers;

    public ShieldLaser shieldLaser;

    public SpawnDoor spawnDoorLeft;

    public SpawnDoor spawnDoorRight;

    public List<LevelWave> waves;

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube((worldBoundsMin + worldBoundsMax) / 2, worldBoundsMax - worldBoundsMin);
    }
}