using UnityEngine;

public class CircularHealthBar : MonoBehaviour
{
    public SpriteRenderer circleSprite;

    public Color defaultColor = Color.white;

    public Color damageColor = Color.white;

    public float damageColorDuration;

    public SpriteMask mask1;

    public SpriteMask mask2;

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
        circleSprite.maskInteraction = lerpedFillAmount < 0.5f ? SpriteMaskInteraction.VisibleOutsideMask : SpriteMaskInteraction.VisibleInsideMask;
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
                circleSprite.color = Color.Lerp(damageColor, defaultColor, (Time.time - lastDamageTime) / damageColorDuration);
            }
            else
            {
                circleSprite.color = defaultColor;
                lastDamageTime = -1;
            }
        }

        // Interpolate a new smoothed fill amount
        lerpedFillAmount = Mathf.Lerp(lerpedFillAmount, fillAmount, 1 - Mathf.Pow(1 - fillAmountLerp, Time.deltaTime));

        // If a change has occured in the interpolated fill amount, update the circular display
        if (Mathf.Abs(lerpedFillAmount - lastLerpedFillAmount) > 0.001f)
        {
            if (lerpedFillAmount < 0.5f && lastLerpedFillAmount >= 0.5f)
            {
                circleSprite.maskInteraction = SpriteMaskInteraction.VisibleOutsideMask;
            }
            else if (lerpedFillAmount >= 0.5f && lastLerpedFillAmount < 0.5f)
            {
                circleSprite.maskInteraction = SpriteMaskInteraction.VisibleInsideMask;
            }

            float fillDeg = Mathf.Clamp01(lerpedFillAmount) * 360;
            if (lerpedFillAmount < 0.5f)
            {
                mask1.transform.localEulerAngles = new Vector3(0, 0, fillDeg);
                mask2.transform.localEulerAngles = new Vector3(0, 0, 180);
            }
            else
            {
                mask1.transform.localEulerAngles = new Vector3(0, 0, 0);
                mask2.transform.localEulerAngles = new Vector3(0, 0, fillDeg - 180);
            }

            lastLerpedFillAmount = lerpedFillAmount;
        }

        lastFillAmount = fillAmount;
    }
}