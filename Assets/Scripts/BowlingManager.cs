using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Manages the bowling UI and player input for bowling the ball.
/// This script controls the accuracy meter, delivery type selection, and spawning the ball.
/// </summary>
public class BowlingManager : MonoBehaviour
{
    [Header("Prefabs & Spawns")]
    [Tooltip("The cricket ball prefab to be instantiated.")]
    public GameObject ballPrefab; 
    [Tooltip("The spawn point for a left-handed bowler.")]
    public Transform leftSpawnPoint; 
    [Tooltip("The spawn point for a right-handed bowler.")]
    public Transform rightSpawnPoint; 
    [Tooltip("The target marker for the ball's bounce point.")]
    public Transform bounceMarker; 

    [Header("UI Meter Settings")]
    [Tooltip("The UI element that moves up and down on the accuracy meter.")]
    public RectTransform meterIndicator;
    [Tooltip("The speed at which the meter indicator moves.")]
    public float meterSpeed = 1.5f;
    [Tooltip("The total height of the accuracy meter in UI units.")]
    public float meterHeight = 200f; 

    // --- State Tracking ---
    private bool isMeterMoving = false;
    
    // Manual control variables for the meter's position and direction.
    private float meterValue = 0.5f;     // Represents the meter's position (0=bottom, 0.5=center, 1=top)
    private int meterDirection = 1;      // 1 moves up, -1 moves down
    
    private CricketBallController.DeliveryType currentType = CricketBallController.DeliveryType.Swing;
    private bool isLeftSide = true; // True for left-handed bowler, false for right.

    /// <summary>
    /// Standard Unity function called every frame.
    /// Used here to update the position of the accuracy meter indicator.
    /// </summary>
    void Update()
    {
        if (isMeterMoving)
        {
            // 1. Manually update the meter's value based on speed and direction.
            meterValue += meterDirection * (meterSpeed * Time.deltaTime);

            // 2. Check for and handle bouncing off the top edge.
            if (meterValue >= 1f)
            {
                meterValue = 1f;       // Clamp to the max value.
                meterDirection = -1;   // Reverse direction to go down.
            }
            // 3. Check for and handle bouncing off the bottom edge.
            else if (meterValue <= 0f)
            {
                meterValue = 0f;       // Clamp to the min value.
                meterDirection = 1;    // Reverse direction to go up.
            }
            
            // 4. Apply the calculated value to the indicator's visual position.
            // Lerp translates the 0-1 meterValue into a Y-coordinate.
            meterIndicator.anchoredPosition = new Vector2(0, Mathf.Lerp(-meterHeight / 2f, meterHeight / 2f, meterValue));
        }
    }

    /// <summary>
    /// Called when the "Swing" button is clicked. Sets the delivery type and starts the meter.
    /// </summary>
    public void OnSwingClicked() 
    { 
        currentType = CricketBallController.DeliveryType.Swing; 
        isMeterMoving = true; 
    }

    /// <summary>
    /// Called when the "Spin" button is clicked. Sets the delivery type and starts the meter.
    /// </summary>
    public void OnSpinClicked() 
    { 
        currentType = CricketBallController.DeliveryType.Spin; 
        isMeterMoving = true; 
    }
    
    /// <summary>
    /// Called when the "Change Side" button is clicked. Toggles between left and right-handed bowling.
    /// </summary>
    public void OnChangeSideClicked() 
    { 
        isLeftSide = !isLeftSide; 
    }

    /// <summary>
    /// Called when the "Bowl" button is clicked. Stops the meter and bowls the ball.
    /// </summary>
    public void OnBowlClicked()
    {
        if (!isMeterMoving) return; // Don't do anything if the meter isn't active.

        // Stop the meter.
        isMeterMoving = false;

        // Calculate the accuracy multiplier based on the final meter position.
        float accuracyMultiplier = CalculateAccuracy(meterValue);
        
        // Determine the active spawn point and bowling direction based on the selected side.
        Transform activeSpawn = isLeftSide ? leftSpawnPoint : rightSpawnPoint;
        int direction = isLeftSide ? 1 : -1; // Left side bowls "inward" (positive), right side "inward" (negative)

        // Instantiate the ball and get its controller component.
        GameObject newBall = Instantiate(ballPrefab, activeSpawn.position, Quaternion.identity);
        CricketBallController controller = newBall.GetComponent<CricketBallController>();
        
        // Initialize and launch the ball with the chosen parameters.
        controller.InitializeAndBowl(bounceMarker.position, currentType, direction, accuracyMultiplier);
    }

    /// <summary>
    /// Calculates the accuracy multiplier based on how close the meter was stopped to the center.
    /// </summary>
    /// <param name="value">The final position of the meter (0-1).</param>
    /// <returns>An accuracy multiplier (0.0f to 1.0f).</returns>
    private float CalculateAccuracy(float value)
    {
        // Calculate the distance from the perfect center (0.5).
        float distFromCenter = Mathf.Abs(value - 0.5f);

        // Return a multiplier based on predefined zones.
        if (distFromCenter <= 0.05f) return 1.0f;       // Perfect (Blue Zone)
        else if (distFromCenter <= 0.15f) return 0.7f;  // Good (Green Zone)
        else if (distFromCenter <= 0.30f) return 0.4f;  // OK (Yellow Zone)
        else return 0.0f;                               // Miss (Red Zone)
    }
}