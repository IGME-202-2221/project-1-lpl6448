using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Controller script for a player's spaceship, which uses player input to move the spaceship around
/// the screen using a basic physics simulation.
/// Author: Luke Lepkowski (lpl6448@rit.edu)
/// </summary>
public class PlayerSpaceship : MonoBehaviour
{
    /// <summary>
    /// The ship's forward acceleration (units/s^2)
    /// </summary>
    public float acceleration;

    /// <summary>
    /// The base angular acceleration (degrees/s^2)
    /// </summary>
    public float baseTurnAcceleration;

    /// <summary>
    /// Max speed at which the ship's velocity will align itself with the forward direction (degrees/s^2)
    /// </summary>
    public float baseTurnToVelocity;

    /// <summary>
    /// Drag coefficient of the space the ship is traveling through
    /// </summary>
    public float drag;

    /// <summary>
    /// Angular drag coefficient of the space the ship is traveling through
    /// </summary>
    public float angularDrag;

    /// <summary>
    /// When breaking, the damper put on the ship
    /// </summary>
    public float breakDamper;

    /// <summary>
    /// When breaking, the amount of deceleration of the ship (units/s^2)
    /// </summary>
    public float breakDeceleration;

    /// <summary>
    /// When breaking, the max speed at which the ship's velocity will align itself with the forward direction (degrees/s^2)
    /// </summary>
    public float breakingTurnToVelocity;

    /// <summary>
    /// When breaking, the added angular acceleration that the ship can turn at (degrees/s^2)
    /// </summary>
    public float breakingTurnAcceleration;

    /// <summary>
    /// Raw input for whether the player is applying the break
    /// </summary>
    private bool breakingInput;

    /// <summary>
    /// Vector2 representing the raw movement input from the player
    /// </summary>
    private Vector2 movementInput;

    /// <summary>
    /// Current velocity (units/s) of the ship
    /// </summary>
    private Vector2 velocity;

    /// <summary>
    /// Current angular velocity (degrees/s) of the ship
    /// </summary>
    private float angularVelocity;

    /// <summary>
    /// The Update function runs a physics simulation, taking into account user input to move
    /// the ship and update its position and rotation on the screen.
    /// </summary>
    private void Update()
    {
        bool isBreaking = movementInput.y < -0.5f || breakingInput;

        // Angularly accelerate or decelerate the ship
        angularVelocity += baseTurnAcceleration * -movementInput.x * Time.deltaTime;
        if (isBreaking)
        {
            angularVelocity += breakingTurnAcceleration * velocity.magnitude * -movementInput.x * Time.deltaTime;
        }

        // Apply angular drag
        angularVelocity *= Mathf.Clamp01(1 - Mathf.Abs(angularVelocity) * angularDrag * Time.deltaTime);

        // Turn the ship
        transform.Rotate(0, 0, angularVelocity * Time.deltaTime);

        // Accelerate or break the ship
        if (!isBreaking)
        {
            velocity += (Vector2)transform.up * movementInput.y * acceleration * Time.deltaTime;
            if (velocity.sqrMagnitude > 0.01f)
            {
                MoveVelocityTowardDirection(baseTurnToVelocity * velocity.magnitude * Time.deltaTime);
            }
        }
        else
        {
            if (velocity.sqrMagnitude > 0.01f)
            {
                velocity *= Mathf.Clamp01(1 - breakDamper * Time.deltaTime - breakDeceleration / velocity.magnitude * Time.deltaTime);
            }
            MoveVelocityTowardDirection(breakingTurnToVelocity * Time.deltaTime);
        }

        // Apply drag
        velocity *= Mathf.Clamp01(1 - velocity.magnitude * drag * Time.deltaTime);

        // Move the ship
        transform.position += (Vector3)velocity * Time.deltaTime;

        // Move the camera if the ship has left the screen (this may be replaced later by collision or a following camera)
        Bounds mapBounds = GetMapBounds();
        if (transform.position.x < mapBounds.min.x)
        {
            Camera.main.transform.position -= Vector3.right * mapBounds.size.x;
        }
        else if (transform.position.x >= mapBounds.max.x)
        {
            Camera.main.transform.position -= Vector3.left * mapBounds.size.x;
        }
        if (transform.position.y < mapBounds.min.y)
        {
            Camera.main.transform.position -= Vector3.up * mapBounds.size.y;
        }
        else if (transform.position.y >= mapBounds.max.y)
        {
            Camera.main.transform.position -= Vector3.down * mapBounds.size.y;
        }
    }

    /// <summary>
    /// Utility function that moves the current velocity's direction a maximum number of degrees
    /// toward the current forward direction of the ship
    /// </summary>
    /// <param name="maxTurnToVelocity">Maximum degrees that the ship can turn during this frame</param>
    private void MoveVelocityTowardDirection(float maxTurnToVelocity)
    {
        float speed = velocity.magnitude;
        Vector2 dir = velocity / speed;

        float currentAngle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg + 90;
        float goalAngle = transform.eulerAngles.z + 180;
        float newAngle = Mathf.MoveTowardsAngle(currentAngle, goalAngle, maxTurnToVelocity);

        dir = new Vector2(Mathf.Sin(newAngle * Mathf.Deg2Rad), -Mathf.Cos(newAngle * Mathf.Deg2Rad));
        velocity = dir * speed;
    }

    /// <summary>
    /// Computes the boundaries of the map, in world space
    /// </summary>
    /// <returns>Bounds object representing the bounds of the screen, in world space</returns>
    private Bounds GetMapBounds()
    {
        float verticalSize = Camera.main.orthographicSize * 2;
        return new Bounds(Camera.main.transform.position, new Vector3(verticalSize * Screen.width / Screen.height, verticalSize));
    }

    /// <summary>
    /// Called by a PlayerInput component to update the player's movement input
    /// </summary>
    /// <param name="context">CallbackContext representing the move action (Vector2)</param>
    public void OnMove(InputAction.CallbackContext context)
    {
        movementInput = context.ReadValue<Vector2>();
    }

    /// <summary>
    /// Called by a PlayerInput component to update whether the player is applying the breaks
    /// </summary>
    /// <param name="context">CallbackContext representing the break action (float)</param>
    public void OnBreak(InputAction.CallbackContext context)
    {
        breakingInput = context.ReadValue<float>() > 0;
    }
}