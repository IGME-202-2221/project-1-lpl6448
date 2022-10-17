using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Basic class that contains data about a wave of enemies
/// Author: Luke Lepkowski (lpl6448@rit.edu)
/// </summary>
public class LevelWave : MonoBehaviour
{
    /// <summary>
    /// Maximum number of enemies from this wave that can be on the map at once
    /// </summary>
    public int maxEnemiesOnMap;

    /// <summary>
    /// List of EnemySpaceship prefabs that will be spawned, in order, during this wave
    /// </summary>
    public List<EnemySpaceship> enemyPrefabs;
}