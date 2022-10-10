using UnityEngine;

/// <summary>
/// Simple class that keeps the world rotation constant for a given object
/// (regardless of the parent's rotation). This is used for the highlight on EnemySpaceships.
/// Author: Luke Lepkowski (lpl6448@rit.edu)
/// </summary>
public class RotationStabilizer : MonoBehaviour
{
    /// <summary>
    /// World euler rotation that the Transform will always be set to
    /// </summary>
    public Vector3 worldEulerAngles;

    /// <summary>
    /// Every frame, updates the euler angles to match the world euler angles,
    /// regardless of the parent's rotation
    /// </summary>
    private void LateUpdate()
    {
        transform.eulerAngles = worldEulerAngles;
    }
}