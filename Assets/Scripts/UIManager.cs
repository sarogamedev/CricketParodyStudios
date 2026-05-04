using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public RectTransform meterIndicator;
    public float meterSpeed = 1.5f; // How fast the meter moves
    
    private float meterValue = 0f;
    private float currentAccuracy = 0f;
    private bool isBowling = false;

    void Update()
    {
        if (!isBowling)
        {
            // Ping pong value between 0 and 1
            meterValue = Mathf.PingPong(Time.time * meterSpeed, 1f);
            
            // Move indicator up and down (assuming a 200px tall background)
            meterIndicator.anchoredPosition = new Vector2(0, Mathf.Lerp(-150f, 150f, meterValue));
            
            // Calculate accuracy: Center (0.5) is 100% accuracy, edges (0 or 1) are 0%
            currentAccuracy = 1f - (Mathf.Abs(meterValue - 0.5f) * 2f);
        }
    }
}