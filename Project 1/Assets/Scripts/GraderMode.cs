using UnityEngine;

/// <summary>
/// The grader's favorite class, which makes the player ship invincible and allows the end of
/// the game to be reached more easily. The game is built to be challenging, so this script
/// allows the end state to be tested without forcing graders to complete the game normally.
/// Author: Luke Lepkowski (lpl6448@rit.edu)
/// </summary>
public class GraderMode : MonoBehaviour
{
    /// <summary>
    /// Reference to the PlayerSpaceship (made invincible when Grader mode is active)
    /// </summary>
    public PlayerSpaceship playerShip;

    /// <summary>
    /// Overlay which is activated when Grader Mode is active
    /// </summary>
    public GameObject overlay;

    /// <summary>
    /// Whether Grader Mode is currently activated
    /// </summary>
    private bool graderMode = false;

    /// <summary>
    /// Toggles the Grader Mode, called from the InputManager.
    /// Makes the playerShip invincible or vulnerable depending on whether Grader Mode is active
    /// </summary>
    public void ToggleGraderMode()
    {
        graderMode = !graderMode;
        overlay.SetActive(graderMode);

        if (playerShip != null)
        {
            playerShip.invincible = graderMode;
        }
    }
}