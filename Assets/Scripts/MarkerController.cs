using UnityEngine;

public class MarkerController : MonoBehaviour
{
    public float speed = 5f;
    
    // Limits to keep the marker on the pitch
    public float minX = -2f;
    public float maxX = 2f;
    public float minZ = -5f;
    public float maxZ = 8f;

    void Update()
    {
        float h = Input.GetAxis("Horizontal"); // A/D keys
        float v = Input.GetAxis("Vertical");   // W/S keys

        Vector3 movement = new Vector3(h, 0, v) * (speed * Time.deltaTime);
        Vector3 newPos = transform.position + movement;

        // Clamp the position so it doesn't leave the pitch
        newPos.x = Mathf.Clamp(newPos.x, minX, maxX);
        newPos.z = Mathf.Clamp(newPos.z, minZ, maxZ);
        
        transform.position = newPos;
    }
}