using UnityEngine;
using System.Collections;

public class ShockwaveAttack : MonoBehaviour
{
    [Header("Shockwave Settings")]
    public float radius = 5f;               // Maximum radius of the shockwave
    public float expansionSpeed = 8f;       // How fast the shockwave expands
    public float maxDamage = 30f;           // Maximum damage at the center
    public float minDamage = 10f;           // Minimum damage at the edge
    public float manaCost = 25f;            // Mana required to use the ability
    public float cooldown = 3f;             // Cooldown time in seconds
    public float heightOffset = 0.1f;       // Height offset from player's position (slightly above ground)
    
    [Header("Healing Effect")]
    public float healingAmount = 15f;         // Amount of health restored when using the ability
    public ParticleSystem healingVFX;         // Optional healing visual effect
    
    // References
    private PlayerStats playerStats;
    private Transform playerTransform;
    
    // State tracking
    private bool canUseShockwave = true;
    private float cooldownTimer = 0f;
    
    void Start()
    {
        playerStats = GetComponent<PlayerStats>();
        playerTransform = transform;
        
        if (playerStats == null)
        {
            Debug.LogError("ShockwaveAttack requires PlayerStats component!");
        }
        
        // Create particle system if not provided
        if (shockwaveParticleSystem == null)
        {
            CreateShockwaveParticleSystem();
        }
    }
    
    void Update()
    {
        // Handle cooldown timer
        if (!canUseShockwave)
        {
            cooldownTimer -= Time.deltaTime;
            if (cooldownTimer <= 0)
            {
                canUseShockwave = true;
            }
        }
        
        // Check for spacebar input
        if (Input.GetKeyDown(KeyCode.Space) && canUseShockwave)
        {
            // Check if player has enough mana
            if (playerStats != null && playerStats.currentMana >= manaCost)
            {
                UseShockwave();
            }
            else
            {
                // Optional: Show feedback that there's not enough mana
                Debug.Log("Not enough mana for shockwave attack!");
            }
        }
    }
    
    void UseShockwave()
    {
        // Consume mana
        playerStats.currentMana -= manaCost;
        
        // Notify UI that mana changed
        if (playerStats.OnManaChanged != null)
        {
            playerStats.OnManaChanged(playerStats.currentMana);
        }
        
        // Heal the player
        HealPlayer();
        
        // Set cooldown
        canUseShockwave = false;
        cooldownTimer = cooldown;
        
        // Start damage application coroutine (which also creates and manages the visual effect)
        StartCoroutine(ApplyShockwaveDamage());
    }
    
    // New method to handle player healing
    void HealPlayer()
    {
        if (playerStats != null)
        {
            // Get current health values
            float currentHealth = playerStats.currentHealth;
            float maxHealth = playerStats.maxHealth;
            
            // Only heal if below max health
            if (currentHealth < maxHealth)
            {
                // Apply healing
                float newHealth = Mathf.Min(currentHealth + healingAmount, maxHealth);
                playerStats.currentHealth = newHealth;
                
                // Show healing VFX if available
                if (healingVFX != null)
                {
                    ParticleSystem healEffect = Instantiate(healingVFX, transform.position, Quaternion.identity);
                    healEffect.transform.parent = transform; // Parent to player
                    Destroy(healEffect.gameObject, 2f);
                }
                
                // Notify UI of health change
                if (playerStats.OnHealthChanged != null)
                {
                    playerStats.OnHealthChanged(playerStats.currentHealth);
                }
                
                Debug.Log($"Player healed for {healingAmount} health. Current health: {playerStats.currentHealth}/{maxHealth}");
            }
        }
    }
    
    [Header("Particle System")]
    public ParticleSystem shockwaveParticleSystem;  // Reference to particle system prefab
    public Color shockwaveColor = new Color(0.2f, 0.4f, 1f, 0.8f);  // Color of the shockwave
    
    IEnumerator ApplyShockwaveDamage()
    {
        float currentRadius = 0;
        float ringWidth = 0.25f; // Thinner damage ring
        float maxRadius = radius * 0.7f; // Reduce max radius
        
        // Create a parent object to hold all the shockwave visuals and follow the player
        GameObject shockwaveParent = new GameObject("ShockwaveFollower");
        shockwaveParent.transform.position = playerTransform.position;
        
        // Position particle system slightly above ground, below player
        Vector3 relativeEffectPosition = new Vector3(0, heightOffset, 0);
        
        // Create instance of particle system as a child of the shockwave parent
        ParticleSystem particleInstance = Instantiate(shockwaveParticleSystem, shockwaveParent.transform);
        particleInstance.transform.localPosition = relativeEffectPosition;
        
        // Set particle system parameters for the shockwave size
        var mainModule = particleInstance.main;
        mainModule.startSpeed = expansionSpeed * 0.8f; // Slightly slower expansion
        mainModule.startLifetime = maxRadius / expansionSpeed + 1.0f; // Add extra time for fadeout
        mainModule.simulationSpace = ParticleSystemSimulationSpace.World;
        
        // Set noise to create non-uniform wave
        var noise = particleInstance.noise;
        noise.enabled = true;
        noise.strength = new ParticleSystem.MinMaxCurve(0.15f);
        noise.frequency = 1.2f;
        
        // Set color with fade out
        var colorModule = particleInstance.colorOverLifetime;
        colorModule.enabled = true;
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[] { 
                new GradientColorKey(new Color(0.2f, 0.7f, 1f), 0.0f), 
                new GradientColorKey(new Color(0.1f, 0.5f, 0.9f), 0.6f),
                new GradientColorKey(new Color(0.0f, 0.4f, 0.8f), 1.0f)
            },
            new GradientAlphaKey[] { 
                new GradientAlphaKey(0.0f, 0.0f),
                new GradientAlphaKey(0.9f, 0.1f),
                new GradientAlphaKey(0.8f, 0.6f), 
                new GradientAlphaKey(0.0f, 1.0f) 
            }
        );
        colorModule.color = gradient;
        
        // Enable size over lifetime for better fade out
        var sizeModule = particleInstance.sizeOverLifetime;
        sizeModule.enabled = true;
        AnimationCurve sizeCurve = new AnimationCurve();
        sizeCurve.AddKey(0.0f, 0.5f);
        sizeCurve.AddKey(0.3f, 0.8f);
        sizeCurve.AddKey(0.7f, 1.0f);
        sizeCurve.AddKey(1.0f, 0.1f);
        sizeModule.size = new ParticleSystem.MinMaxCurve(1f, sizeCurve);
        
        // Play the effect
        particleInstance.Play();
        
        // Active damage phase
        while (currentRadius < maxRadius)
        {
            // Update the shockwave parent to follow the player
            shockwaveParent.transform.position = new Vector3(
                playerTransform.position.x,
                playerTransform.position.y,
                playerTransform.position.z
            );
            
            // Expand the radius based on time and speed
            currentRadius += expansionSpeed * 0.8f * Time.deltaTime;
            
            // Calculate inner and outer radius of the donut/ring
            float innerRadius = Mathf.Max(0, currentRadius - ringWidth);
            float outerRadius = currentRadius;
            
            // Find all enemies within the current radius ring
            Collider[] hitColliders = Physics.OverlapSphere(playerTransform.position, outerRadius, LayerMask.GetMask("Enemy"));
            
            foreach (Collider hitCollider in hitColliders)
            {
                // Skip if this is the player's collider
                if (hitCollider.transform == playerTransform)
                    continue;
                
                // Get distance from player
                float distance = Vector3.Distance(playerTransform.position, hitCollider.transform.position);
                
                // Only affect enemies within the ring
                if (distance >= innerRadius && distance <= outerRadius)
                {
                    // Try to get enemy component
                    BaseEnemy enemy = hitCollider.GetComponent<BaseEnemy>();
                    
                    if (enemy != null)
                    {
                        // Calculate damage based on distance (full damage throughout the ring)
                        float distanceFactor = 1 - (distance / maxRadius);
                        float damage = Mathf.Lerp(minDamage, maxDamage, distanceFactor);
                        
                        // Apply damage to enemy
                        enemy.TakeDamage(damage);
                        
                        // Add knockback effect
                        Rigidbody enemyRb = enemy.GetComponent<Rigidbody>();
                        if (enemyRb != null)
                        {
                            Vector3 direction = (enemy.transform.position - playerTransform.position).normalized;
                            enemyRb.AddForce(direction * 8f * distanceFactor, ForceMode.Impulse);
                        }
                        
                        // Tag this enemy as already hit to prevent multiple damage
                        StartCoroutine(MarkEnemyAsHit(enemy));
                    }
                }
            }
            
            yield return null;
        }
        
        // Stop emitting new particles after the damage phase
        particleInstance.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        
        // Allow existing particles to fade out naturally
        while (particleInstance.IsAlive(true))
        {
            // Continue to update position during fadeout
            shockwaveParent.transform.position = playerTransform.position;
            yield return null;
        }
        
        // Clean up
        Destroy(shockwaveParent);
    }
    
    IEnumerator MarkEnemyAsHit(BaseEnemy enemy)
    {
        // Add a temporary component to mark this enemy as hit
        if (enemy.gameObject.GetComponent<ShockwaveHitMarker>() == null)
        {
            ShockwaveHitMarker marker = enemy.gameObject.AddComponent<ShockwaveHitMarker>();
            
            // Remove the marker after a delay
            yield return new WaitForSeconds(0.5f);
            
            if (marker != null && enemy != null)
            {
                Destroy(marker);
            }
        }
    }
    
    // Helper class to mark enemies that have already been hit
    private class ShockwaveHitMarker : MonoBehaviour { }
    
    // Create a default particle system if none is provided
    private void CreateShockwaveParticleSystem()
    {
        // Create a new GameObject with particle system
        GameObject particleObj = new GameObject("ShockwaveParticleSystem");
        ParticleSystem particleSystem = particleObj.AddComponent<ParticleSystem>();
        
        // Configure base particle system
        var mainModule = particleSystem.main;
        mainModule.duration = 1.0f;
        mainModule.loop = false;
        mainModule.startLifetime = 1.0f;
        mainModule.startSpeed = expansionSpeed;
        mainModule.startSize = 0.1f; // Smaller particle size
        mainModule.startColor = shockwaveColor;
        mainModule.simulationSpace = ParticleSystemSimulationSpace.World;
        mainModule.maxParticles = 500; // More particles for smoother look
        
        // Configure emission
        var emission = particleSystem.emission;
        emission.rateOverTime = 0;
        emission.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0.0f, 200) }); // More particles
        
        // Configure shape for donut/ring - smoother ring
        var shape = particleSystem.shape;
        shape.shapeType = ParticleSystemShapeType.Donut;
        shape.radius = 0.05f; // Smaller initial radius
        shape.radiusThickness = 0.1f; // Slight thickness to avoid perfect ring
        shape.arc = 360f;
        
        // Configure size over lifetime
        var sizeOverLifetime = particleSystem.sizeOverLifetime;
        sizeOverLifetime.enabled = true;
        AnimationCurve sizeOverLifetimeCurve = new AnimationCurve();
        sizeOverLifetimeCurve.AddKey(0.0f, 0.5f);
        sizeOverLifetimeCurve.AddKey(0.3f, 1.0f);
        sizeOverLifetimeCurve.AddKey(0.7f, 1.2f);
        sizeOverLifetimeCurve.AddKey(1.0f, 0.1f);
        sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, sizeOverLifetimeCurve);
        
        // Add noise for less uniformity
        var noise = particleSystem.noise;
        noise.enabled = true;
        noise.strength = new ParticleSystem.MinMaxCurve(0.1f);
        noise.frequency = 0.8f;
        noise.quality = ParticleSystemNoiseQuality.High;
        
        // Configure color over lifetime for fade out
        var colorOverLifetime = particleSystem.colorOverLifetime;
        colorOverLifetime.enabled = true;
        Gradient colorGradient = new Gradient();
        colorGradient.SetKeys(
            new GradientColorKey[] { 
                new GradientColorKey(new Color(0.2f, 0.6f, 1f, 1f), 0.0f), 
                new GradientColorKey(new Color(0.1f, 0.5f, 0.9f, 1f), 0.5f),
                new GradientColorKey(new Color(0.0f, 0.3f, 0.8f, 1f), 1.0f) 
            },
            new GradientAlphaKey[] { 
                new GradientAlphaKey(0.0f, 0.0f), 
                new GradientAlphaKey(0.9f, 0.1f),
                new GradientAlphaKey(0.9f, 0.7f),
                new GradientAlphaKey(0.0f, 1.0f) 
            }
        );
        colorOverLifetime.color = colorGradient;
        
        // Add rotation over lifetime for more natural movement
        var rotationOverLifetime = particleSystem.rotationOverLifetime;
        rotationOverLifetime.enabled = true;
        rotationOverLifetime.z = new ParticleSystem.MinMaxCurve(-30f, 30f);
        
        // Add particle system renderer
        var renderer = particleObj.GetComponent<ParticleSystemRenderer>();
        renderer.renderMode = ParticleSystemRenderMode.Billboard;
        
        // Create a softer material for the particles
        Material particleMaterial = new Material(Shader.Find("Particles/Standard Unlit"));
        particleMaterial.SetColor("_Color", shockwaveColor);
        particleMaterial.SetFloat("_InvFade", 1.5f); // Softer particles
        renderer.material = particleMaterial;
        
        // Store reference to the particle system
        shockwaveParticleSystem = particleSystem;
        
        // Make the object a prefab
        particleObj.SetActive(false);
        DontDestroyOnLoad(particleObj);
    }
}