using UnityEngine;
using TMPro;

public class ScoreAddition : MonoBehaviour
{
    public TextMeshPro scoreText;

    public Vector2 velocity;

    public float sustainTime;

    public float decayTime;

    private float startTime;

    public void SetScore(int points)
    {
        scoreText.text = "+" + points.ToString("N0");
    }

    private void Start()
    {
        startTime = Time.time;
    }

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