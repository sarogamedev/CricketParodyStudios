using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class CricketBallController : MonoBehaviour
{
    public enum DeliveryType { Swing, Spin }
    public enum BallState { Idle, InAir, PostBounce }

    [Header("Physics Settings")]
    public float swingStrength = 15f;
    public float spinStrength = 25f;
    public float timeToTarget = 0.8f; 

    private DeliveryType currentType;
    private BallState currentState = BallState.Idle;
    
    private int lateralDirectionMultiplier;
    private float accuracyMultiplier;
    private Vector3 fixedLateralDirection; 
    private Rigidbody rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic; 
    }

    public void InitializeAndBowl(Vector3 targetPos, DeliveryType type, int direction, float accuracy)
    {
        currentType = type;
        lateralDirectionMultiplier = direction;
        accuracyMultiplier = accuracy;

        // 1. Calculate the straight-line velocity required to hit the target
        Vector3 distance = targetPos - transform.position;
        Vector3 initialVelocity = new Vector3(
            distance.x / timeToTarget,
            (distance.y - 0.5f * Physics.gravity.y * Mathf.Pow(timeToTarget, 2)) / timeToTarget,
            distance.z / timeToTarget
        );

        // 2. Lock the lateral (Left/Right) direction relative to this path
        fixedLateralDirection = Vector3.Cross(initialVelocity.normalized, Vector3.up).normalized;

        // 3. KINEMATIC SWING COMPENSATION
        // If we apply swing, it will push the ball off the target. 
        // We must calculate that exact drift and aim slightly wide to compensate.
        if (currentType == DeliveryType.Swing)
        {
            float finalForce = swingStrength * lateralDirectionMultiplier * accuracyMultiplier;
            
            // Acceleration = Force / Mass
            float lateralAcceleration = finalForce / rb.mass;
            
            // Kinematic formula: v = -0.5 * a * t
            float velocityCorrectionMagnitude = -0.5f * lateralAcceleration * timeToTarget;
            
            // Add this correction angle to our initial throw
            initialVelocity += fixedLateralDirection * velocityCorrectionMagnitude;
        }

        // 4. Launch the ball
        rb.linearVelocity = initialVelocity;
        currentState = BallState.InAir;
        
        Destroy(gameObject, 8f); 
    }

    void FixedUpdate()
    {
        // Apply the continuous curve force only while in the air
        if (currentState == BallState.InAir && currentType == DeliveryType.Swing)
        {
            float finalForce = swingStrength * lateralDirectionMultiplier * accuracyMultiplier;
            rb.AddForce(fixedLateralDirection * finalForce, ForceMode.Force);
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        // Trigger the bounce transitions
        if (currentState == BallState.InAir && collision.gameObject.CompareTag("Pitch"))
        {
            currentState = BallState.PostBounce;

            // Spin doesn't need air compensation because the force is ONLY applied at the bounce
            if (currentType == DeliveryType.Spin)
            {
                Vector3 impactLateralDir = Vector3.Cross(rb.linearVelocity.normalized, Vector3.up).normalized;
                float snapForce = spinStrength * lateralDirectionMultiplier * accuracyMultiplier;
                rb.AddForce(impactLateralDir * snapForce, ForceMode.VelocityChange);
            }
        }
    }
}