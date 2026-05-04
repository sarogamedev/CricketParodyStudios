using UnityEngine;
using UnityEngine.UI;

public class BowlingManager : MonoBehaviour
{
    [Header("Prefabs & Spawns")]
    public GameObject ballPrefab; 
    public Transform leftSpawnPoint; 
    public Transform rightSpawnPoint; 
    public Transform bounceMarker; 

    [Header("UI Meter Settings")]
    public RectTransform meterIndicator;
    public float meterSpeed = 1.5f;
    public float meterHeight = 200f; 

    // State Tracking
    private bool isMeterMoving = false;
    
    // NEW: Manual control variables instead of Mathf.PingPong
    private float meterValue = 0.5f;     // Starts in the middle
    private int meterDirection = 1;      // 1 moves up, -1 moves down
    
    private CricketBallController.DeliveryType currentType = CricketBallController.DeliveryType.Swing;
    private bool isLeftSide = true;

    void Update()
    {
        if (isMeterMoving)
        {
            // 1. Manually add or subtract time based on our current direction
            meterValue += meterDirection * (meterSpeed * Time.deltaTime);

            // 2. Bounce off the top edge
            if (meterValue >= 1f)
            {
                meterValue = 1f;       // Clamp to max
                meterDirection = -1;   // Reverse direction to go down
            }
            // 3. Bounce off the bottom edge
            else if (meterValue <= 0f)
            {
                meterValue = 0f;       // Clamp to min
                meterDirection = 1;    // Reverse direction to go up
            }
            
            // 4. Apply the position visually
            meterIndicator.anchoredPosition = new Vector2(0, Mathf.Lerp(-meterHeight / 2f, meterHeight / 2f, meterValue));
        }
    }

    public void OnSwingClicked() 
    { 
        currentType = CricketBallController.DeliveryType.Swing; 
        isMeterMoving = true; 
    }

    public void OnSpinClicked() 
    { 
        currentType = CricketBallController.DeliveryType.Spin; 
        isMeterMoving = true; 
    }
    
    public void OnChangeSideClicked() 
    { 
        isLeftSide = !isLeftSide; 
    }

    public void OnBowlClicked()
    {
        if (!isMeterMoving) return; 

        // Freeze the meter exactly where it is. 
        // Because we aren't using Time.time, it will sit perfectly still.
        isMeterMoving = false;

        // Calculate the multiplier based on the manually tracked value
        float accuracyMultiplier = CalculateAccuracy(meterValue);
        
        // Determine spawn point and direction
        Transform activeSpawn = isLeftSide ? leftSpawnPoint : rightSpawnPoint;
        int direction = isLeftSide ? 1 : -1;

        // Instantiate and launch the ball
        GameObject newBall = Instantiate(ballPrefab, activeSpawn.position, Quaternion.identity);
        CricketBallController controller = newBall.GetComponent<CricketBallController>();
        
        controller.InitializeAndBowl(bounceMarker.position, currentType, direction, accuracyMultiplier);
    }

    private float CalculateAccuracy(float value)
    {
        float distFromCenter = Mathf.Abs(value - 0.5f);

        if (distFromCenter <= 0.05f) return 1.0f;       // Blue Zone
        else if (distFromCenter <= 0.15f) return 0.7f;  // Green Zone 
        else if (distFromCenter <= 0.30f) return 0.4f;  // Yellow Zone 
        else return 0.0f;                               // Red Zone 
    }
}