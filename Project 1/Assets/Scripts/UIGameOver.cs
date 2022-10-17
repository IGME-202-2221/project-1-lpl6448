using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

/// <summary>
/// UI class for the Game Over panel that appears when the player loses or wins.
/// Author: Luke Lepkowski (lpl6448@rit.edu)
/// </summary>
public class UIGameOver : MonoBehaviour
{
    /// <summary>
    /// Reference to the Animator that controls the "in" animation for the panel
    /// </summary>
    public Animator animator;

    /// <summary>
    /// Reference to the UI text that displays the player's score
    /// </summary>
    public TextMeshProUGUI scoreText;

    /// <summary>
    /// Reference to the UI text that displays the player's progress in the level
    /// </summary>
    public TextMeshProUGUI progressText;

    /// <summary>
    /// Color that progressText is set to if the player wins
    /// </summary>
    public Color winProgressTextColor;

    /// <summary>
    /// Animates the Game Over panel in and displays the score, level progress, and win state
    /// </summary>
    /// <param name="score">Final score of the game</param>
    /// <param name="level">Final level that the player progressed to</param>
    /// <param name="won">Whether the player won</param>
    public void In(int score, int level, bool won)
    {
        scoreText.text = score.ToString("N0");

        if (won)
        {
            progressText.text = "You escaped!";
            progressText.color = winProgressTextColor;
        }
        else
        {
            progressText.text = "You made it to level " + (level + 1);
        }

        animator.SetTrigger("In");
    }

    /// <summary>
    /// Called when the player clicks the "Play Again" button, reloading the scene
    /// </summary>
    public void PlayAgain()
    {
        SceneManager.LoadScene(1);
    }

    /// <summary>
    /// Called when the player clicks the "Main Menu" button, loading the Menu scene
    /// </summary>
    public void MainMenu()
    {
        SceneManager.LoadScene(0);
    }
}