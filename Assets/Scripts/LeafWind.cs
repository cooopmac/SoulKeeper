using UnityEngine;

public class LeafWind : MonoBehaviour
{
    public float windSpeed = 0.1f; // Speed of wind movement
    public float rotationSpeed = 50f; // Speed of random rotation
    private Vector3 windDirection;

    void Start()
    {
        // Randomize initial wind direction for variation
        windDirection = new Vector3(Random.Range(-1f, 1f), 0f, Random.Range(-1f, 1f)).normalized;
    }

    void Update()
    {
        // Apply wind movement (horizontal drift in X and Z)
        transform.Translate(windDirection * windSpeed * Time.deltaTime, Space.World);

        // Add random rotation for fluttering effect
        transform.Rotate(0, 0, Random.Range(-rotationSpeed, rotationSpeed) * Time.deltaTime);

        // Optional: Reverse wind direction occasionally for gusts
        if (Random.value < 0.01f) // 1% chance per frame to change direction
        {
            windDirection = new Vector3(Random.Range(-1f, 1f), 0f, Random.Range(-1f, 1f)).normalized;
        }
    }
}