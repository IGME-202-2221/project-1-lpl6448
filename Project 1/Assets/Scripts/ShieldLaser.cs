using UnityEngine;
using System.Collections;

public class ShieldLaser : MonoBehaviour
{
    public SpriteRenderer laserFull;

    public SpriteRenderer laserLeft;

    public SpriteRenderer laserRight;

    public float secondsToDeactivate;

    public void Deactivate()
    {
        StartCoroutine(DeactivateCrt());
    }

    IEnumerator DeactivateCrt()
    {
        laserFull.enabled = false;
        laserLeft.enabled = true;
        laserRight.enabled = true;

        float startTime = Time.time;
        while (Time.time - startTime < secondsToDeactivate)
        {
            float t = (Time.time - startTime) / secondsToDeactivate;
            laserLeft.size = new Vector2((1 - t) * 16, 0.5f);
            laserLeft.transform.localPosition = new Vector3((1 + t) * -8, 0);
            laserRight.size = new Vector2((1 - t) * 16, 0.5f);
            laserRight.transform.localPosition = new Vector3((1 + t) * 8, 0);

            yield return null;
        }

        laserLeft.enabled = false;
        laserRight.enabled = false;
    }
}