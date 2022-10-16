using UnityEngine;
using UnityEngine.UI;

public class UILevelCircle : MonoBehaviour
{
    public Image partialFill;

    public Image fullFill;

    public void DisplayLevel(int state)
    {
        partialFill.enabled = state >= 1;
        fullFill.enabled = state >= 2;
    }
}