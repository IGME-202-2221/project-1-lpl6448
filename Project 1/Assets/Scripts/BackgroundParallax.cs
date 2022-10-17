using UnityEngine;

/// <summary>
/// Class that creates a parallax layer (that moves at a rate different from the foreground)
/// Author: Luke Lepkowski (lpl6448@rit.edu)
/// </summary>
public class BackgroundParallax : MonoBehaviour
{
    /// <summary>
    /// Transform that the parallax effect uses for distance (typically the Camera)
    /// </summary>
    public Transform positionReference;

    /// <summary>
    /// Number that the reference position is multiplied by to get the parallax position
    /// </summary>
    [Range(0, 1)]
    public float distanceMultiplier = 1;

    /// <summary>
    /// Every frame (after any Camera movement), move this parallax layer to the reference position multiplied by the distanceMultiplier
    /// </summary>
    private void LateUpdate()
    {
        Vector3 pos = positionReference.position * distanceMultiplier;
        pos.z = transform.position.z;
        transform.position = pos;
    }
}