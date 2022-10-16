using UnityEngine;
using TMPro;

public class UIScoreDisplay : MonoBehaviour
{
    public TextMeshProUGUI scoreText;

    public float scoreLerp;

    public int score;

    private float lerpedScore = -1;

    private void Update()
    {
        lerpedScore = Mathf.Lerp(lerpedScore, score, 1 - Mathf.Pow(1 - scoreLerp, Time.deltaTime));
        Display(Mathf.CeilToInt(lerpedScore));
    }

    private void Display(int score)
    {
        scoreText.text = score.ToString("N0");
    }
}