using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// UI class representing an unfilled, partially filled, or fully filled circle,
/// depending on which level the player is on.
/// Author: Luke Lepkowski (lpl6448@rit.edu)
/// </summary>
public class UILevelCircle : MonoBehaviour
{
    /// <summary>
    /// Image enabled if this level is in progress
    /// </summary>
    public Image partialFill;

    /// <summary>
    /// Image enabled if this level has been beaten
    /// </summary>
    public Image fullFill;

    /// <summary>
    /// Updates the image fills based on the state.
    /// State 0 = unfilled
    /// State 1 = partially filled
    /// State 2 = fully filled
    /// </summary>
    /// <param name="state">State (0-2) of this level circle</param>
    public void DisplayLevel(int state)
    {
        partialFill.enabled = state >= 1;
        fullFill.enabled = state >= 2;
    }
}