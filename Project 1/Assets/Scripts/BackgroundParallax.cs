using UnityEngine;

public class BackgroundParallax : MonoBehaviour
{
    public Transform positionReference;

    [Range(0, 1)]
    public float distanceMultiplier = 1;

    private void LateUpdate()
    {
        Vector3 pos = positionReference.position * distanceMultiplier;
        pos.z = transform.position.z;
        transform.position = pos;
    }
}