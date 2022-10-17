using UnityEngine;
using TMPro;

/// <summary>
/// Represents a score addition effect, which animates the amount of score added
/// (from killing an enemy or deactivating a shield station) and then slowly fades it out.
/// Author: Luke Lepkowski (lpl6448@rit.edu)
/// </summary>
public class ScoreAddition : MonoBehaviour
{
    /// <summary>
    /// Reference to the TextMeshPro that shows the amount of score added
    /// </summary>
    public TextMeshPro scoreText;

    /// <summary>
    /// Constant velocity (units/s) of the effect
    /// </summary>
    public Vector2 velocity;

    /// <summary>
    /// Amount of time (seconds) that the effect lingers before beginning to fade
    /// </summary>
    public float sustainTime;

    /// <summary>
    /// Amount of time (seconds) that the effect fades out for
    /// </summary>
    public float decayTime;

    /// <summary>
    /// Timestamp (seconds) that this effect was instantiated at
    /// </summary>
    private float startTime;

    /// <summary>
    /// Sets the scoreText display to show the amount of score added
    /// </summary>
    /// <param name="score">Score to be displayed on the scoreText</param>
    public void SetScore(int score)
    {
        scoreText.text = "+" + score.ToString("N0");
    }

    /// <summary>
    /// When instantiated, initialize the startTime to the current frame's timestamp
    /// </summary>
    private void Start()
    {
        startTime = Time.time;
    }

    /// <summary>
    /// Every frame, move the effect according to its velocity and fade it out
    /// (and eventually destroy it) according to sustainTime and decayTime
    /// </summary>
    private void Update()
    {
        transform.Translate(velocity * Time.deltaTime);

        float time = Time.time - startTime;
        if (time > sustainTime && time < sustainTime + decayTime)
        {
            scoreText.alpha = 1 - (time - sustainTime) / decayTime;
        }
        else if (time > sustainTime + decayTime)
        {
            Destroy(gameObject);
        }
    }
}