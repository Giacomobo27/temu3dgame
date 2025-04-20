using UnityEngine;


//useless


public class DayNightCycle : MonoBehaviour
{
    [Tooltip("Duration of a full day-night cycle in real-world seconds.")]
    [SerializeField]
    private float dayDurationSeconds = 120f; // Default to 2 minutes

    [Tooltip("Optional: Set a specific initial time of day (0.0 = sunrise, 0.25 = noon, 0.5 = sunset, 0.75 = midnight).")]
    [Range(0f, 1f)]
    [SerializeField]
    private float initialTimeOfDay = 0.0f;

    // Keep track of the current time progression (0.0 to 1.0)
    private float currentTimeOfDay = 0.0f;

    // Store the initial rotation on the Y and Z axes to preserve them
    private float initialYRotation;
    private float initialZRotation;

    void Start()
    {
        // Store the starting Y and Z rotation angles set in the editor
        initialYRotation = transform.eulerAngles.y;
        initialZRotation = transform.eulerAngles.z;

        // Set the initial time
        currentTimeOfDay = initialTimeOfDay;

        // Apply the initial rotation based on the starting time
        UpdateSunRotation();
    }

    void Update()
    {
        // Prevent division by zero or negative duration
        if (dayDurationSeconds <= 0)
        {
            dayDurationSeconds = 0.1f; // Set a minimum small duration
            Debug.LogWarning("Day Duration Seconds must be positive. Setting to 0.1s.");
        }

        // Calculate how much time has passed since the last frame,
        // scaled by the desired day duration.
        float timeIncrement = Time.deltaTime / dayDurationSeconds;

        // Update the current time of day
        currentTimeOfDay += timeIncrement;

        // Use Mathf.Repeat to wrap the time back to 0 after it reaches 1
        currentTimeOfDay = Mathf.Repeat(currentTimeOfDay, 1.0f);

        // Update the sun's rotation based on the current time
        UpdateSunRotation();
    }

    void UpdateSunRotation()
    {
        // Calculate the sun's rotation around the X-axis (pitch)
        // Map the 0-1 time range to a full 0-360 degree rotation
        float sunAngleX = currentTimeOfDay * 360f;

        // Apply the rotation
        // We rotate around the local X-axis, preserving the initial Y and Z rotations
        // This typically simulates the sun rising/setting across the horizon.
        transform.localRotation = Quaternion.Euler(sunAngleX, initialYRotation, initialZRotation);
    }

    // Optional: If you want to be able to set the time from another script
    public void SetTimeOfDay(float timeNormalized)
    {
        currentTimeOfDay = Mathf.Clamp01(timeNormalized); // Ensure time is between 0 and 1
        UpdateSunRotation();
    }
}