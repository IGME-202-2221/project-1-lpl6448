using UnityEngine;
using System.Collections;

/// <summary>
/// Represents the laser or shield keeping the player from escaping the prison. Once all shield stations
/// are deactivated, the laser does a deactivation animation, allowing the player ship to go to the next level.
/// Author: Luke Lepkowski (lpl6448@rit.edu)
/// </summary>
public class ShieldLaser : MonoBehaviour
{
    /// <summary>
    /// SpriteRenderer that is enabled when this laser is intact
    /// </summary>
    public SpriteRenderer laserFull;

    /// <summary>
    /// SpriteRenderer of the left laser, used during the deactivation animation
    /// </summary>
    public SpriteRenderer laserLeft;

    /// <summary>
    /// SpriteRenderer of the right laser, used during the deactivation animation
    /// </summary>
    public SpriteRenderer laserRight;

    /// <summary>
    /// Duration (in seconds) of the deactivation animation
    /// </summary>
    public float secondsToDeactivate;

    /// <summary>
    /// Runs the deactivation animation of this shield laser
    /// </summary>
    public void Deactivate()
    {
        StartCoroutine(DeactivateCrt());
    }

    /// <summary>
    /// Disables laserFull and animates laserLeft and laserRight to the edges
    /// of the screen, giving the appearance that the laser is being deactivated
    /// </summary>
    /// <returns>IEnumerator for the Unity coroutine</returns>
    private IEnumerator DeactivateCrt()
    {
        laserFull.enabled = false;
        laserLeft.enabled = true;
        laserRight.enabled = true;

        float startTime = Time.time;
        while (Time.time - startTime < secondsToDeactivate)
        {
            float t = (Time.time - startTime) / secondsToDeactivate;
            laserLeft.size = new Vector2((1 - t) * 16, 0.5f);
            laserLeft.transform.localPosition = new Vector3((1 + t) * -8, 0);
            laserRight.size = new Vector2((1 - t) * 16, 0.5f);
            laserRight.transform.localPosition = new Vector3((1 + t) * 8, 0);

            yield return null;
        }

        laserLeft.enabled = false;
        laserRight.enabled = false;
    }
}