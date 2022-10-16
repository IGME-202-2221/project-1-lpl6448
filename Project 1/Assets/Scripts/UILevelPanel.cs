using UnityEngine;

public class UILevelPanel : MonoBehaviour
{
    public UILevelCircle[] levelCircles;

    public void SetActiveLevel(int level)
    {
        for (int i = 0; i < levelCircles.Length; i++)
        {
            levelCircles[i].DisplayLevel(Mathf.Clamp(level - i, -1, 1) + 1);
        }
    }
}