using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// UI class for the main menu, which loads the Game scene when the "Play"
/// button is pressed.
/// Author: Luke Lepkowski (lpl6448@rit.edu)
/// </summary>
public class UIMainMenu : MonoBehaviour
{
    /// <summary>
    /// Loads the Game scene when the "Play" button is pressed
    /// </summary>
    public void PlayGame()
    {
        SceneManager.LoadScene(1);
    }
}