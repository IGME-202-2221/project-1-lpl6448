using UnityEngine;

/// <summary>
/// Superclass of all enemy spaceships in the game, containing a target/following Transform reference
/// and some initialization code for enemy-specific logic in the GameManager.
/// Author: Luke Lepkowski (lpl6448@rit.edu)
/// </summary>
public abstract class EnemySpaceship : Spaceship
{
    /// <summary>
    /// Transform that this ship will follow and shoot at
    /// </summary>
    public Transform target;

    /// <summary>
    /// When this enemy is created, add this spaceship to the game world's list of enemies and to the collision world
    /// </summary>
    private void Start()
    {
        gameManager.AddEnemy(this);
        gameManager.physicsWorld.AddObject(this);
    }

    /// <summary>
    /// When this enemy is destroyed/killed, remove it from the game world and from the collision world
    /// </summary>
    private void OnDestroy()
    {
        gameManager.RemoveEnemy(this);
        gameManager.physicsWorld.RemoveObject(this);
    }
}