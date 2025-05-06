using UnityEngine;

public class PlayerAiming : MonoBehaviour
{
    [Header("Aiming Settings")]
    public float aimSmoothness = 5f;
    public bool showAimIndicator = true;
    public float aimIndicatorDistance = 2f;
    public float targetSmoothTime = 0.1f; // Smoothing for aim target
    public float minimumAimDistance = 0.1f; // Minimum threshold to prevent micromovements

    [Header("Shooting Settings")]
    public GameObject shadowBoltPrefab;
    public Transform firePoint;
    public float baseFireRate = 3f;
    public float currentFireRate;
    public float baseDamage = 10f;
    public float currentDamage;
    
    public bool autoFire = true;

    [Header("Visual Effects")]
    public ParticleSystem muzzleFlash;
    public ParticleSystem muzzleSmoke;
    
    [Header("References")]
    public Transform aimIndicator;
    private PlayerStats playerStats;
    
    private Camera mainCamera;
    private Plane groundPlane;
    private float fireTimer = 0f;
    private CrosshairController crosshair;
    private Animator animator;
    private bool isShooting = false;
    
    // For smooth aim targeting
    private Vector3 currentAimTarget = Vector3.zero;
    private Vector3 targetAimPosition = Vector3.zero;
    
    void Start()
    {
        // Limit frame rate for more consistent behavior
        Application.targetFrameRate = 60;
        
        mainCamera = Camera.main;
        
        // Find crosshair
        crosshair = FindFirstObjectByType<CrosshairController>();
        
        // Find animator (either on this object or on a child)
        animator = GetComponent<Animator>();
        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }
        
        // Get player stats
        playerStats = GetComponent<PlayerStats>();
        if (playerStats == null)
        {
            playerStats = FindFirstObjectByType<PlayerStats>();
        }
        
        // Create fire point if needed
        if (firePoint == null)
        {
            GameObject fp = new GameObject("FirePoint");
            fp.transform.parent = transform;
            fp.transform.localPosition = new Vector3(0, 0.5f, 0.5f);
            firePoint = fp.transform;
        }
        
        if (showAimIndicator && aimIndicator == null)
        {
            CreateAimIndicator();
        }
        
        // Initialize attack stats
        UpdateAttackStats();
        
        // Initialize target position
        targetAimPosition = transform.position + transform.forward * aimIndicatorDistance;
    }

    void Update()
    {
        HandleShootingInput();
    }
    
    void FixedUpdate()
    {
        HandleAiming();
    }
    
    void HandleAiming()
    {
        // Get mouse position in world space
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        
        // Use a fixed height plane that follows the player rather than world origin
        Plane playerGroundPlane = new Plane(Vector3.up, new Vector3(0, transform.position.y, 0));
        
        if (playerGroundPlane.Raycast(ray, out float rayDistance))
        {
            Vector3 pointOnGround = ray.GetPoint(rayDistance);
            
            // Smooth the target position
            targetAimPosition = Vector3.SmoothDamp(targetAimPosition, pointOnGround, ref currentAimTarget, targetSmoothTime);
            
            // Calculate direction to look at (flat on XZ plane)
            Vector3 direction = targetAimPosition - transform.position;
            direction.y = 0;
            
            // Only rotate if the direction is significant (prevents jitter from tiny movements)
            if (direction.magnitude > minimumAimDistance)
            {
                // Rotate player to face mouse position - use fixedDeltaTime for consistent rotation speed
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, aimSmoothness * Time.fixedDeltaTime);
                
                // Update aim indicator position if it exists
                if (aimIndicator != null && showAimIndicator)
                {
                    aimIndicator.position = transform.position + transform.forward * aimIndicatorDistance;
                }
            }
        }
    }
    
    void HandleShootingInput()
    {
        // Mouse button down - start shooting
        if (Input.GetMouseButtonDown(0))
        {
            isShooting = true;
            if (animator != null)
            {
                animator.SetTrigger("Shoot");
                animator.SetBool("isShooting", true);
            }
            
            // Fire the first shot immediately
            FireShadowBolt();
            fireTimer = 1f / currentFireRate;
        }
        
        // Mouse button up - stop shooting
        if (Input.GetMouseButtonUp(0))
        {
            isShooting = false;
            if (animator != null)
            {
                animator.SetBool("isShooting", false);
            }
        }
        
        // Handle continuous firing if button is still held
        if (isShooting && autoFire)
        {
            // Update fire timer
            fireTimer -= Time.deltaTime;
            
            // Check if we can fire again based on rate
            if (fireTimer <= 0)
            {
                FireShadowBolt();
                fireTimer = 1f / currentFireRate;
            }
        }
    }
    
    void FireShadowBolt()
    {
        // Create shadow bolt at firepoint position
        GameObject bolt = Instantiate(shadowBoltPrefab, firePoint.position, Quaternion.identity);
    
        // Set bolt direction and damage
        ShadowBolt shadowBoltComponent = bolt.GetComponent<ShadowBolt>();
        if (shadowBoltComponent != null)
        {
            shadowBoltComponent.SetDirection(transform.forward);
            shadowBoltComponent.damage = currentDamage; // Set damage based on player's ATT stat
        }
        else
        {
            // Fallback if component is missing
            Rigidbody rb = bolt.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.linearVelocity = transform.forward * 20f;
            }
        }
    
        // Play visual effects
        if (muzzleFlash != null)
        {
            muzzleFlash.Play();
        }

        if (muzzleSmoke != null)
        {
            muzzleSmoke.Play();
        }
        
        // Visual feedback on crosshair
        if (crosshair != null)
        {
            crosshair.FlashOnShoot();
        }
    }
    
    private void CreateAimIndicator()
    {
        // Create a simple aim indicator
        GameObject indicator = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        indicator.transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);
        
        // Remove collider as it's just visual
        Destroy(indicator.GetComponent<Collider>());
        
        // Create and assign material with your neon pink color
        Material indicatorMat = new Material(Shader.Find("Standard"));
        indicatorMat.color = new Color(1f, 0.18f, 0.48f); // Neon pink approximation
        indicator.GetComponent<Renderer>().material = indicatorMat;
        
        // Set as child of player but store transform reference
        indicator.transform.parent = transform;
        aimIndicator = indicator.transform;
    }
    
    public Vector3 GetAimDirection()
    {
        return transform.forward;
    }
    
    // Called by the UpgradeManager when attack or dexterity stats change
    public void UpdateAttackStats()
    {
        if (playerStats != null)
        {
            // Calculate damage boost from ATT stat (10% per level)
            float damageMultiplier = 1f + (playerStats.attackStat - 1) * 0.1f;
            currentDamage = baseDamage * damageMultiplier;
            
            // Calculate fire rate boost from DEX stat (7% per level)
            float fireRateMultiplier = 1f + (playerStats.dexterityStat - 1) * 0.07f;
            currentFireRate = baseFireRate * fireRateMultiplier;
            
            Debug.Log($"Attack stats updated: Damage = {currentDamage} (x{damageMultiplier}), Fire Rate = {currentFireRate}/sec (x{fireRateMultiplier})");
        }
        else
        {
            // Default values if no PlayerStats found
            currentDamage = baseDamage;
            currentFireRate = baseFireRate;
        }
    }
}