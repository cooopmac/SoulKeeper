using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float baseMoveSpeed = 5f;
    public float currentMoveSpeed;
    public float rotationSpeed = 10f;

    [Header("References")]
    private Rigidbody rb;
    private Animator animator;
    private PlayerStats playerStats;
    
    // Add this to track current velocity
    private Vector3 currentVelocity = Vector3.zero;
    private Vector3 targetVelocity = Vector3.zero;
    public float smoothTime = 0.1f;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        
        // Look for animator in children if not on this object
        animator = GetComponent<Animator>();
        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
            Debug.Log("Found animator in children: " + (animator != null));
        }
        
        // Add this to fix the jittering
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        
        // Get player stats
        playerStats = GetComponent<PlayerStats>();
        if (playerStats == null)
        {
            playerStats = FindFirstObjectByType<PlayerStats>();
        }
        
        // Initialize movement speed
        UpdateMoveSpeed();
    }

    void Update()
    {
        // Check movement
        bool isWalking = Input.GetAxis("Horizontal") != 0 || Input.GetAxis("Vertical") != 0;
        
        // Set animator parameters
        if (animator != null)
        {
            animator.SetBool("isWalking", isWalking);
        }
    }

    void FixedUpdate()
    {
        // Get movement input
        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");

        // Calculate movement direction in world space
        Vector3 movement = new Vector3(horizontalInput, 0f, verticalInput).normalized;
        
        // Calculate raw movement magnitude directly from input
        float inputMagnitude = movement.magnitude;
        
        // Set target velocity
        targetVelocity = movement * currentMoveSpeed;
        
        // Smoothly approach the target velocity
        Vector3 smoothVelocity = Vector3.SmoothDamp(
            rb.linearVelocity, 
            targetVelocity, 
            ref currentVelocity, 
            smoothTime
        );
        
        // Move using velocity
        rb.linearVelocity = new Vector3(smoothVelocity.x, rb.linearVelocity.y, smoothVelocity.z);
        
        // Update animator with INPUT magnitude rather than velocity
        if (animator != null)
        {
            // Use input magnitude directly instead of calculated velocity
            animator.SetFloat("Speed", inputMagnitude);
        }
    }
    
    // This will be called by the UpgradeManager when the speed stat changes
    public void UpdateMoveSpeed()
    {
        if (playerStats != null)
        {
            // Calculate speed bonus: 5% per speed level
            float speedMultiplier = 1f + (playerStats.speedStat - 1) * 0.05f;
            currentMoveSpeed = baseMoveSpeed * speedMultiplier;
            
            Debug.Log("Movement speed updated: " + currentMoveSpeed + " (Base: " + baseMoveSpeed + ", Multiplier: " + speedMultiplier + ")");
        }
        else
        {
            // If no player stats, use base speed
            currentMoveSpeed = baseMoveSpeed;
            Debug.Log("No PlayerStats found, using base move speed: " + baseMoveSpeed);
        }
    }
}