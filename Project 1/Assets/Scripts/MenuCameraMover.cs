using UnityEngine;

/// <summary>
/// Component that moves this GameObject with a particular velocity, used to animate
/// the Camera in the main menu.
/// Author: Luke Lepkowski (lpl6448@rit.edu)
/// </summary>
public class MenuCameraMover : MonoBehaviour
{
    /// <summary>
    /// Velocity of the Transform (units/s)
    /// </summary>
    public Vector2 velocity;

    /// <summary>
    /// Every frame, moves this Transform according to the velocity
    /// </summary>
    private void Update()
    {
        transform.Translate(velocity * Time.deltaTime);
    }
}