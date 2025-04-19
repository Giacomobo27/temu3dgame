using UnityEngine;

public class FixedFollowCamera : MonoBehaviour
{
    [Tooltip("The target object the camera should follow (Assign your Robot)")]
    public Transform target;

    [Tooltip("How far behind the target the camera should be")]
    public Vector3 offset = new Vector3(0f, 2f, -3f); // Default: 2 units up, 5 units back

    // Store the initial desired Y position or calculate based on offset
    private float fixedYPosition;
    private float fixedXPosition;

    void Start()
    {
        if (target == null)
        {
            Debug.LogError("FixedFollowCamera: Target not assigned!", this);
            enabled = false; // Disable script if no target
            return;
        }

        fixedXPosition = transform.position.x;
        fixedYPosition = transform.position.y;
        
        Vector3 initialPosition = new Vector3(
            fixedXPosition,
            fixedYPosition,
            target.position.z + offset.z // Only Z uses target + offset
        );
        transform.position = initialPosition;
    }


    // Use LateUpdate to ensure the target has finished its movement for the frame
    void LateUpdate()
    {
        if (target == null) return; // Exit if target is lost

        Vector3 newPosition = new Vector3(
            fixedXPosition,
            fixedYPosition,
            target.position.z + offset.z // Only Z tracks the target
        );

        // --- Apply the position directly (No Lerp needed for fixed axes) ---
        transform.position = newPosition;

    }
}