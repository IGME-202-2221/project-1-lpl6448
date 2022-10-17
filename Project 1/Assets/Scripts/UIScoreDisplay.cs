using UnityEngine;
using TMPro;

/// <summary>
/// UI class for the score display/panel, which displays the current score and
/// smoothly updates any changes to the score.
/// Author: Luke Lepkowski (lpl6448@rit.edu)
/// </summary>
public class UIScoreDisplay : MonoBehaviour
{
    /// <summary>
    /// Reference to the UI text that displays the current (lerped/smoothed) score
    /// </summary>
    public TextMeshProUGUI scoreText;

    /// <summary>
    /// Interpolation amount that the score is lerped/smoothed by (0-1 percentage that the smoothed score
    /// approaches after one second)
    /// </summary>
    public float scoreLerp;

    /// <summary>
    /// Current score of the game
    /// </summary>
    public int score;

    /// <summary>
    /// Lerped/smoothed score, interpolated according to scoreLerp
    /// </summary>
    private float lerpedScore = -1;

    /// <summary>
    /// Every frame, interpolate the lerpedScore and update the score display
    /// </summary>
    private void Update()
    {
        lerpedScore = Mathf.Lerp(lerpedScore, score, 1 - Mathf.Pow(1 - scoreLerp, Time.deltaTime));
        Display(Mathf.CeilToInt(lerpedScore));
    }

    /// <summary>
    /// Updates the scoreText to reflect the given score
    /// </summary>
    /// <param name="score">Score to display on the scoreText</param>
    private void Display(int score)
    {
        scoreText.text = score.ToString("N0");
    }
}