using UnityEngine;

/// <summary>
/// Manages the player-controlled marker to set the ball's bounce position.
/// This script reads keyboard input to move the marker within defined boundaries on the pitch.
/// </summary>
public class MarkerController : MonoBehaviour
{
    [Tooltip("The speed at which the marker moves.")]
    public float speed = 5f;
    
    [Header("Movement Boundaries")]
    [Tooltip("The minimum X-axis position (left boundary).")]
    public float minX = -2f;
    [Tooltip("The maximum X-axis position (right boundary).")]
    public float maxX = 2f;
    [Tooltip("The minimum Z-axis position (closest to bowler).")]
    public float minZ = -5f;
    [Tooltip("The maximum Z-axis position (closest to batsman).")]
    public float maxZ = 8f;

    /// <summary>
    /// Standard Unity function called every frame.
    /// Used here to handle player input and update the marker's position.
    /// </summary>
    void Update()
    {
        // Get input from the horizontal (A/D, Left/Right arrows) and vertical (W/S, Up/Down arrows) axes.
        float h = Input.GetAxis("Horizontal"); 
        float v = Input.GetAxis("Vertical");   

        // Calculate the movement vector for this frame.
        Vector3 movement = new Vector3(h, 0, v) * (speed * Time.deltaTime);
        
        // Calculate the new potential position.
        Vector3 newPos = transform.position + movement;

        // Clamp the new position to ensure it stays within the defined boundaries.
        newPos.x = Mathf.Clamp(newPos.x, minX, maxX);
        newPos.z = Mathf.Clamp(newPos.z, minZ, maxZ);
        
        // Apply the final, clamped position to the marker.
        transform.position = newPos;
    }
}