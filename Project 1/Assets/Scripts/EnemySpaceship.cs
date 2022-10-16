using UnityEngine;

public class EnemySpaceship : Spaceship
{
    /// <summary>
    /// Transform that this ship will follow and shoot at
    /// </summary>
    public Transform target;

    /// <summary>
    /// When the game starts, add this spaceship to the game world's list of enemies and to the collision world
    /// </summary>
    private void Start()
    {
        gameManager.AddEnemy(this);
        gameManager.physicsWorld.AddObject(this);
    }

    private void OnDestroy()
    {
        gameManager.RemoveEnemy(this);
        gameManager.physicsWorld.RemoveObject(this);
    }
}