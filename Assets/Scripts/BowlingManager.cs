using System;
using UnityEngine;
using UnityEngine.UI;

public class BowlingManager : MonoBehaviour
{
    [Header("Prefabs & Spawns")]
    public GameObject ballPrefab; // Your Ball Prefab (MUST have CricketBallController attached)
    public Transform leftSpawnPoint; // Empty GameObject, NO COLLIDER
    public Transform rightSpawnPoint; // Empty GameObject, NO COLLIDER
    public Transform bounceMarker; // The UI Target on the pitch

    [Header("UI Meter Settings")]
    public RectTransform meterIndicator;
    public float meterSpeed = 1.5f;
    public float meterHeight = 200f; // Height of your UI meter background

    // State Tracking
    private bool isMeterMoving = false;
    private float meterValue = 0.5f; 
    private CricketBallController.DeliveryType currentType = CricketBallController.DeliveryType.Swing;
    private bool isLeftSide = true;

    private void Start()
    {
        isMeterMoving = false;
    }

    private void Update()
    {
        // The meter ONLY moves after a type is selected and BEFORE the bowl button is clicked
        if (isMeterMoving)
        {
            meterValue = Mathf.PingPong(Time.time * meterSpeed, 1f);
            // Move indicator up and down visually
            meterIndicator.anchoredPosition = new Vector2(0, Mathf.Lerp(-meterHeight / 2f, meterHeight / 2f, meterValue));
        }
    }

    // --- BUTTON CLICKS ---

    public void OnSwingClicked() 
    { 
        currentType = CricketBallController.DeliveryType.Swing; 
        isMeterMoving = true; // Start the meter
    }

    public void OnSpinClicked() 
    { 
        currentType = CricketBallController.DeliveryType.Spin; 
        isMeterMoving = true; // Start the meter
    }
    
    public void OnChangeSideClicked() 
    { 
        isLeftSide = !isLeftSide; 
        Debug.Log("Switched to: " + (isLeftSide ? "Left" : "Right") + " side.");
    }

    public void OnBowlClicked()
    {
        // Ignore if we haven't selected a type yet or if we already bowled
        if (!isMeterMoving) return; 

        // 1. INSTANTLY FREEZE THE METER
        isMeterMoving = false;

        // 2. Calculate the multiplier based exactly on where it stopped
        float accuracyMultiplier = CalculateAccuracy(meterValue);
        Debug.Log("Meter stopped at " + meterValue + ". Accuracy Multiplier: " + accuracyMultiplier);

        // 3. Determine spawn point and swing/spin direction multiplier (-1 or 1)
        Transform activeSpawn = isLeftSide ? leftSpawnPoint : rightSpawnPoint;
        int direction = isLeftSide ? 1 : -1;

        // 4. Instantiate and launch the ball
        GameObject newBall = Instantiate(ballPrefab, activeSpawn.position, Quaternion.identity);
        CricketBallController controller = newBall.GetComponent<CricketBallController>();
        
        controller.InitializeAndBowl(bounceMarker.position, currentType, direction, accuracyMultiplier);
    }

    // --- MATH ---

    // Maps the 0-1 meter value to the exact PDF accuracy tiers
    private float CalculateAccuracy(float value)
    {
        float distFromCenter = Mathf.Abs(value - 0.5f);

        if (distFromCenter <= 0.05f) return 1.0f;       // Blue Zone (Perfect)
        else if (distFromCenter <= 0.15f) return 0.7f;  // Green Zone (Good)
        else if (distFromCenter <= 0.30f) return 0.4f;  // Yellow Zone (Okay)
        else return 0.0f;                               // Red Zone (No movement)
    }
}