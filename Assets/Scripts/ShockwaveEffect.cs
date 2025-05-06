using UnityEngine;

public class ShockwaveEffect : MonoBehaviour
{
    public float expansionSpeed = 8f;
    public float maxSize = 5f;
    public Color startColor = new Color(0.2f, 0.4f, 1f, 0.8f);
    public Color endColor = new Color(0.2f, 0.4f, 1f, 0f);
    
    private float currentSize = 0f;
    private Material material;
    private MeshRenderer meshRenderer;
    private float startTime;
    
    void Start()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        if (meshRenderer != null)
        {
            material = meshRenderer.material;
        }
        
        transform.localScale = Vector3.zero;
        startTime = Time.time;
    }
    
    void Update()
    {
        // Calculate progress (0 to 1)
        float progress = (Time.time - startTime) * expansionSpeed / maxSize;
        
        if (progress < 1.0f)
        {
            // Expand the effect
            currentSize = maxSize * progress;
            transform.localScale = new Vector3(currentSize, 0.1f, currentSize);
            
            // Update color
            if (material != null)
            {
                material.color = Color.Lerp(startColor, endColor, progress);
            }
        }
        else
        {
            // Done expanding
            Destroy(gameObject);
        }
    }
}