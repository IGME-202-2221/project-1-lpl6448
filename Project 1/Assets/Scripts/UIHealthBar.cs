using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Horizontally filled UI health bar used for the player spaceship.
/// This script also controls damage flashes to the health bar.
/// Author: Luke Lepkowski (lpl6448@rit.edu)
/// </summary>
public class UIHealthBar : MonoBehaviour
{
    /// <summary>
    /// Image that is filled according to the fillAmount
    /// </summary>
    public Image fillImage;

    /// <summary>
    /// RectTransform that is moved to the edge of the filled bar
    /// </summary>
    public RectTransform shadowTransform;

    /// <summary>
    /// Default color (when not being damaged)
    /// </summary>
    public Color defaultColor = Color.white;

    /// <summary>
    /// Damage flash color
    /// </summary>
    public Color damageColor = Color.white;

    /// <summary>
    /// Amount of time that this health bar flashes for when damage is taken
    /// </summary>
    public float damageColorDuration;

    /// <summary>
    /// Current fill amount of this health bar (0-1)
    /// </summary>
    [Range(0, 1)]
    public float fillAmount;

    /// <summary>
    /// Lerp factor for the fill amount (0-1 percentage that the lerped/smoothed amount approaches after one second)
    /// </summary>
    public float fillAmountLerp;

    /// <summary>
    /// Last frame's fill amount
    /// </summary>
    private float lastFillAmount = -1;

    /// <summary>
    /// Current lerped/smoothed fill amount
    /// </summary>
    private float lerpedFillAmount;

    /// <summary>
    /// Last frame's lerped/smoothed fill amount
    /// </summary>
    private float lastLerpedFillAmount = -1;

    /// <summary>
    /// Timestamp (seconds) of the last time this health bar took damage
    /// </summary>
    private float lastDamageTime = -1;

    /// <summary>
    /// To initialize the health bar, set the lerped fill amount
    /// </summary>
    private void Start()
    {
        lerpedFillAmount = fillAmount;
    }

    /// <summary>
    /// Every frame, update the lerpedFillAmount, and if a change in the fill amounts is detected,
    /// update the fill effect by changing fillImage's fill amount. Also, apply any damage flashes.
    /// </summary>
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