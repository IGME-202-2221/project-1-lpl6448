using UnityEngine;

/// <summary>
/// Health bar used on enemy spaceships, containing a circular/radial fill for sprites.
/// This script also controls damage flashes to the health bar.
/// Author: Luke Lepkowski (lpl6448@rit.edu)
/// </summary>
public class CircularHealthBar : MonoBehaviour
{
    /// <summary>
    /// Reference to the SpriteRenderer of this health bar
    /// </summary>
    public SpriteRenderer circleSprite;

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
    /// Reference to the first mask used for the circular fill effect
    /// </summary>
    public SpriteMask mask1;

    /// <summary>
    /// Reference to the second mask used for the circular fill effect
    /// </summary>
    public SpriteMask mask2;
    
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
    /// To initialize the health bar, set the lerped fill amount and initialize the masks
    /// </summary>
    private void Start()
    {
        lerpedFillAmount = fillAmount;
        circleSprite.maskInteraction = lerpedFillAmount < 0.5f ? SpriteMaskInteraction.VisibleOutsideMask : SpriteMaskInteraction.VisibleInsideMask;
    }

    /// <summary>
    /// Every frame, update the lerpedFillAmount, and if a change in the fill amounts is detected,
    /// update the radial fill effect by changing the positions of the masks. Also, apply any damage flashes.
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