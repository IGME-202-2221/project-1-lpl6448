using UnityEngine;
using UnityEngine.UI;

public class UIHealthBar : MonoBehaviour
{
    public Image fillImage;

    public RectTransform shadowTransform;

    public Color defaultColor = Color.white;

    public Color damageColor = Color.white;

    public float damageColorDuration;

    [Range(0, 1)]
    public float fillAmount;

    public float fillAmountLerp;

    private float lastFillAmount = -1;

    private float lerpedFillAmount;

    private float lastLerpedFillAmount = -1;

    private float lastDamageTime = -1;

    private void Start()
    {
        lerpedFillAmount = fillAmount;
    }
    private void Update()
    {
        // If the health went down, begin the damage animation
        if (fillAmount < lastFillAmount)
        {
            lastDamageTime = Time.time;
        }

        // Update the color of the sprite according to the damage animation
        if (lastDamageTime != -1)
        {
            if (Time.time - lastDamageTime < damageColorDuration)
            {
                fillImage.color = Color.Lerp(damageColor, defaultColor, (Time.time - lastDamageTime) / damageColorDuration);
            }
            else
            {
                fillImage.color = defaultColor;
                lastDamageTime = -1;
            }
        }

        // Interpolate a new smoothed fill amount
        lerpedFillAmount = Mathf.Lerp(lerpedFillAmount, fillAmount, 1 - Mathf.Pow(1 - fillAmountLerp, Time.deltaTime));

        // If a change has occured in the interpolated fill amount, update the UI display
        if (Mathf.Abs(lerpedFillAmount - lastLerpedFillAmount) > 0.001f)
        {
            fillImage.fillAmount = lerpedFillAmount;
            shadowTransform.anchorMin = new Vector2(lerpedFillAmount, 0);
            shadowTransform.anchorMax = new Vector2(lerpedFillAmount, 1);

            lastLerpedFillAmount = lerpedFillAmount;
        }

        lastFillAmount = fillAmount;
    }
}