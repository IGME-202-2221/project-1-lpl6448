using UnityEngine;

/// <summary>
/// UI class representing the level panel, containing LevelCircles that
/// are updated to reflect the player's current level.
/// Author: Luke Lepkowski (lpl6448@rit.edu)
/// </summary>
public class UILevelPanel : MonoBehaviour
{
    /// <summary>
    /// Array of LevelCircles that are updated to reflect the current level
    /// </summary>
    public UILevelCircle[] levelCircles;

    /// <summary>
    /// Updates all of the levelCircles to reflect the current level
    /// </summary>
    /// <param name="level">Index of the active level</param>
    public void SetActiveLevel(int level)
    {
        for (int i = 0; i < levelCircles.Length; i++)
        {
            levelCircles[i].DisplayLevel(Mathf.Clamp(level - i, -1, 1) + 1);
        }
    }
}