using UnityEngine;

/// <summary>
/// Manages the physics and behavior of the cricket ball after it has been bowled.
/// This script handles swing and spin dynamics, applying forces to simulate ball movement.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class CricketBallController : MonoBehaviour
{
    /// <summary>
    /// Defines the type of delivery, which determines the physics applied.
    /// </summary>
    public enum DeliveryType { Swing, Spin }

    /// <summary>
    /// Represents the current state of the ball during its trajectory.
    /// </summary>
    public enum BallState { Idle, InAir, PostBounce }

    [Header("Physics Settings")]
    [Tooltip("How strongly the ball swings in the air.")]
    public float swingStrength = 15f;
    [Tooltip("How sharply the ball turns after bouncing (for spin).")]
    public float spinStrength = 25f;
    [Tooltip("The fixed time the ball will take to reach the bounce marker.")]
    public float timeToTarget = 0.8f; 

    // Private state variables
    private DeliveryType currentType;
    private BallState currentState = BallState.Idle;
    
    private int lateralDirectionMultiplier; // -1 for left, 1 for right
    private float accuracyMultiplier; // 0 to 1, affects how much swing/spin is applied
    private Vector3 fixedLateralDirection; 
    private Rigidbody rb;

    /// <summary>
    /// Standard Unity function called when the script instance is being loaded.
    /// Used to get the Rigidbody component and set its properties.
    /// </summary>
    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic; 
    }

    /// <summary>
    /// Initializes the ball's properties and launches it towards the target.
    /// </summary>
    /// <param name="targetPos">The position the ball should aim for (the bounce marker).</param>
    /// <param name="type">The type of delivery (Swing or Spin).</param>
    /// <param name="direction">The lateral direction of swing/spin (-1 or 1).</param>
    /// <param name="accuracy">The accuracy multiplier (0-1).</param>
    public void InitializeAndBowl(Vector3 targetPos, DeliveryType type, int direction, float accuracy)
    {
        currentType = type;
        lateralDirectionMultiplier = direction;
        accuracyMultiplier = accuracy;

        // 1. Calculate the straight-line velocity required to hit the target in the specified time.
        Vector3 distance = targetPos - transform.position;
        Vector3 initialVelocity = new Vector3(
            distance.x / timeToTarget,
            (distance.y - 0.5f * Physics.gravity.y * Mathf.Pow(timeToTarget, 2)) / timeToTarget,
            distance.z / timeToTarget
        );

        // 2. Lock the lateral (Left/Right) direction relative to the initial velocity vector.
        // This ensures swing is always perpendicular to the ball's path.
        fixedLateralDirection = Vector3.Cross(initialVelocity.normalized, Vector3.up).normalized;

        // 3. KINEMATIC SWING COMPENSATION
        // If we apply a continuous swing force, the ball will drift off its target.
        // We must pre-calculate this drift and aim slightly wider to compensate, ensuring it still hits the marker.
        if (currentType == DeliveryType.Swing)
        {
            float finalForce = swingStrength * lateralDirectionMultiplier * accuracyMultiplier;
            
            // a = F/m
            float lateralAcceleration = finalForce / rb.mass;
            
            // Kinematic formula: v = -0.5 * a * t (calculates the velocity needed to counteract the drift)
            float velocityCorrectionMagnitude = -0.5f * lateralAcceleration * timeToTarget;
            
            // Add this corrective velocity to our initial launch.
            initialVelocity += fixedLateralDirection * velocityCorrectionMagnitude;
        }

        // 4. Launch the ball with the calculated initial velocity.
        rb.linearVelocity = initialVelocity;
        currentState = BallState.InAir;
        
        // Destroy the ball after a set time to clean up the scene.
        Destroy(gameObject, 8f); 
    }

    /// <summary>
    /// Standard Unity function called at a fixed interval, used for physics calculations.
    /// </summary>
    void FixedUpdate()
    {
        // Apply the continuous swing force while the ball is in the air.
        if (currentState == BallState.InAir && currentType == DeliveryType.Swing)
        {
            float finalForce = swingStrength * lateralDirectionMultiplier * accuracyMultiplier;
            rb.AddForce(fixedLateralDirection * finalForce, ForceMode.Force);
        }
    }

    /// <summary>
    /// Standard Unity function called when this collider/rigidbody has begun touching another rigidbody/collider.
    /// </summary>
    /// <param name="collision">The collision data associated with this event.</param>
    void OnCollisionEnter(Collision collision)
    {
        // Transition from InAir to PostBounce state when hitting the pitch.
        if (currentState == BallState.InAir && collision.gameObject.CompareTag("Pitch"))
        {
            currentState = BallState.PostBounce;

            // Apply spin force *at the moment of impact*.
            // This creates a sharp change in direction.
            if (currentType == DeliveryType.Spin)
            {
                Vector3 impactLateralDir = Vector3.Cross(rb.linearVelocity.normalized, Vector3.up).normalized;
                float snapForce = spinStrength * lateralDirectionMultiplier * accuracyMultiplier;
                rb.AddForce(impactLateralDir * snapForce, ForceMode.VelocityChange);
            }
        }
    }
}