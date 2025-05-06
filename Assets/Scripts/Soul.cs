using UnityEngine;
using System.Collections;

public class Soul : MonoBehaviour
{
    public float lifetime = 5f; // How long the soul lasts before despawning
    public float flashStartTime = 3f; // When to start flashing (seconds before despawn)
    public float flashSpeed = 10f; // Speed of the flashing effect
    public Color originalColor = new Color(0.5f, 0.5f, 1f); // Default soul color
    public Color flashColor = Color.white; // Color to flash to
    
    private Rigidbody rb;
    private Renderer[] renderers;
    private float despawnTime;
    private bool isFlashing = false;
    
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        renderers = GetComponentsInChildren<Renderer>();
        despawnTime = Time.time + lifetime;
        
        // Set initial color
        SetColor(originalColor);
        
        // Schedule destruction
        Destroy(gameObject, lifetime); 
    }
    
    void Update()
    {
        // Start flashing when approaching the end of lifetime
        float timeRemaining = despawnTime - Time.time;
        
        if (timeRemaining <= flashStartTime && !isFlashing)
        {
            isFlashing = true;
            StartCoroutine(FlashRoutine());
        }
    }
    
    void OnCollisionEnter(Collision collision) 
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerStats playerStats = collision.gameObject.GetComponent<PlayerStats>();
            if (playerStats != null)
            {
                playerStats.CollectSoul();
                Destroy(gameObject);
            }
        }
        else if (collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            if (rb != null)
            {
                rb.isKinematic = false;
                rb.useGravity = false;
                rb.linearVelocity = Vector3.zero;
            }
        }
    }
    
    // Set color for all renderers
    void SetColor(Color color)
    {
        foreach (Renderer renderer in renderers)
        {
            // For standard materials
            if (renderer.material.HasProperty("_Color"))
            {
                renderer.material.color = color;
            }
            
            // For emissive materials
            if (renderer.material.HasProperty("_EmissionColor"))
            {
                renderer.material.SetColor("_EmissionColor", color * 0.5f);
            }
        }
    }
    
    // Coroutine for flashing effect
    IEnumerator FlashRoutine()
    {
        while (true)
        {
            // Calculate flash intensity using a sine wave
            float flashIntensity = Mathf.Abs(Mathf.Sin(Time.time * flashSpeed));
            
            // Increase flash frequency as time runs out
            float timeRemaining = despawnTime - Time.time;
            if (timeRemaining < 1f)
            {
                flashSpeed = 20f; // Flash faster in last second
            }
            
            // Lerp between original and flash color
            Color currentColor = Color.Lerp(originalColor, flashColor, flashIntensity);
            SetColor(currentColor);
            
            yield return null;
        }
    }
}