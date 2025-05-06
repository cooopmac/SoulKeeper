using UnityEngine;

public class RimLightPositioner : MonoBehaviour
{
    public Transform target;         // Your character
    public Transform cameraTransform; // Main camera
    public float distanceFromTarget = 2.0f;
    public float heightOffset = 1.0f;
    public bool invertPosition = false;

    void LateUpdate()
    {
        if (target == null || cameraTransform == null)
            return;
            
        // Get direction from camera to target
        Vector3 directionFromCamera = (target.position - cameraTransform.position).normalized;
        
        // If inverted, use opposite direction (for backlight effect)
        if (invertPosition)
            directionFromCamera = -directionFromCamera;
            
        // Position the rim light behind the character relative to camera
        Vector3 targetPosition = target.position + directionFromCamera * distanceFromTarget;
        
        // Add height offset
        targetPosition.y += heightOffset;
        
        // Update position
        transform.position = targetPosition;
        
        // Make the light look at the character
        transform.LookAt(target);
    }
}