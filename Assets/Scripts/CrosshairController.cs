using UnityEngine;
using UnityEngine.UI;

public class CrosshairController : MonoBehaviour 
{
    public float pulseSpeed = 1.5f;
    public float pulseAmount = 0.2f;
    
    private Image crosshairImage;
    private RectTransform rectTransform;
    private Canvas parentCanvas;
    private float baseSize;
    
    void Start() 
    {
        crosshairImage = GetComponent<Image>();
        rectTransform = GetComponent<RectTransform>();
        parentCanvas = GetComponentInParent<Canvas>();
        baseSize = rectTransform.sizeDelta.x;
        
        // Hide the system cursor
        Cursor.visible = false;
    }
    
    void Update() 
    {
        // Update position to follow mouse
        Vector2 mousePosition = Input.mousePosition;
        rectTransform.position = mousePosition;
        
        // Simple pulse effect
        float pulse = 1 + Mathf.Sin(Time.time * pulseSpeed) * pulseAmount;
        rectTransform.sizeDelta = new Vector2(baseSize * pulse, baseSize * pulse);
    }
    
    // You can call this from PlayerController when firing
    public void FlashOnShoot() 
    {
        crosshairImage.color = Color.white;
        Invoke("ResetColor", 0.05f);
    }
    
    private void ResetColor() 
    {
        crosshairImage.color = new Color(1f, 0.18f, 0.48f); // Neon Pink
    }
}