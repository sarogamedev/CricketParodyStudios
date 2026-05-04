using UnityEngine;

public class CricketBallController : MonoBehaviour
{
    public enum DeliveryType { Swing, Spin }
    public enum BallState { Idle, InAir, PostBounce }

    [Header("Settings")]
    public float swingStrength = 20f;
    public float spinStrength = 25f;
    public float timeToTarget = 0.8f; 

    private DeliveryType currentType;
    private BallState currentState = BallState.Idle;
    private int lateralDirectionMultiplier;
    private float accuracyMultiplier;
    
    private Rigidbody rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    // Called by BowlingManager right after instantiation
    public void InitializeAndBowl(Vector3 targetPos, DeliveryType type, int direction, float accuracy)
    {
        currentType = type;
        lateralDirectionMultiplier = direction;
        accuracyMultiplier = accuracy;

        // Calculate trajectory
        Vector3 distance = targetPos - transform.position;
        Vector3 initialVelocity = new Vector3(
            distance.x / timeToTarget,
            (distance.y - 0.5f * Physics.gravity.y * Mathf.Pow(timeToTarget, 2)) / timeToTarget,
            distance.z / timeToTarget
        );

        rb.linearVelocity = initialVelocity;
        currentState = BallState.InAir;
        
        // Destroy ball after 10 seconds to keep scene clean
        Destroy(gameObject, 10f); 
    }

    void FixedUpdate()
    {
        if (currentState == BallState.InAir && currentType == DeliveryType.Swing)
        {
            Vector3 lateralDir = Vector3.Cross(rb.linearVelocity.normalized, Vector3.up);
            float finalForce = swingStrength * lateralDirectionMultiplier * accuracyMultiplier;
            rb.AddForce(lateralDir * finalForce, ForceMode.Force);
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (currentState == BallState.InAir && collision.gameObject.CompareTag("Pitch"))
        {
            currentState = BallState.PostBounce;

            if (currentType == DeliveryType.Spin)
            {
                Vector3 lateralDir = Vector3.Cross(rb.linearVelocity.normalized, Vector3.up);
                float snapForce = spinStrength * lateralDirectionMultiplier * accuracyMultiplier;
                rb.AddForce(lateralDir * snapForce, ForceMode.VelocityChange);
            }
        }
    }
}