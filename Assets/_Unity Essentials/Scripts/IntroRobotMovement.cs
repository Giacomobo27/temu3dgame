using UnityEngine;

public class IntroRobotMovement : MonoBehaviour
{
    [Tooltip("How fast the robot moves forward after the initial delay (units per second)")]
    public float forwardSpeed = 2.0f; // Adjust this value to match the run animation's apparent speed

    [Tooltip("Wait time in seconds before the robot starts moving forward")]
    public float initialDelay = 5.0f;

    [Tooltip("Optional: Set a duration in seconds for how long the robot should move after the delay. Set to 0 or negative to run forever.")]
    public float moveDuration = 10.0f;

    private float delayTimer = 0f;      // Timer to track the initial delay
    private float moveTimer = 0f;       // Timer to track movement duration after delay
    private bool hasStartedMoving = false; // Flag to indicate if the delay is over

    void Update()
    {
        // --- Phase 1: Initial Delay ---
        if (!hasStartedMoving)
        {
            // Increment the delay timer
            delayTimer += Time.deltaTime;

            // Check if the delay time has passed
            if (delayTimer >= initialDelay)
            {
                hasStartedMoving = true; // Set the flag to true
                Debug.Log("Initial delay finished. Robot starting to move.");
            }
            // IMPORTANT: Do not proceed to movement logic during the delay
            return; // Exit Update early if still in delay phase
        }

        // --- Phase 2: Movement (Only runs if hasStartedMoving is true) ---

        // Check if movement should stop based on duration
        bool shouldStop = (moveDuration > 0 && moveTimer >= moveDuration);

        if (!shouldStop)
        {
            // Move the robot forward along its local Z-axis
            transform.Translate(Vector3.forward * forwardSpeed * Time.deltaTime);

            // Increment the movement timer only if moveDuration is set
            if (moveDuration > 0)
            {
                moveTimer += Time.deltaTime;
                // Optional: Log exactly when duration is reached
                if(moveTimer >= moveDuration)
                {
                    Debug.Log("Movement duration reached.");
                }
            }
        }
        // else: Robot stops moving if duration is reached
    }
}