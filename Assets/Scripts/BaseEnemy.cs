using UnityEngine;

public abstract class BaseEnemy : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 3f;
    public float rotationSpeed = 5f;
    
    [Header("Enemy Stats")]
    public int maxHealth = 30;
    public float currentHealth;
    public int damageToPlayer = 0;
    public int pointValue = 10;
    
    [Header("Combat")]
    public float attackRange = 2f;
    public float attackCooldown = 2f;
    public float attackDuration = 1f;
    
    [Header("Effects")]
    public GameObject deathEffectPrefab;
    public GameObject damageEffectPrefab;
    
    [Header("Soul Drop")]
    public GameObject soulPrefab;
    
    // References
    protected Transform playerTransform;
    protected Rigidbody rb;
    protected SimpleEnemyAnimator simpleAnimator;
    
    // State tracking
    protected bool isDead = false;
    protected bool isAttacking = false;
    protected float nextAttackTime = 0f;
    protected float distanceToPlayer = float.MaxValue;
    
    protected virtual void Awake()
    {
        // Initialize components
        rb = GetComponent<Rigidbody>();
        simpleAnimator = GetComponent<SimpleEnemyAnimator>();
        
        if (simpleAnimator == null)
        {
            Debug.LogWarning(gameObject.name + " is missing SimpleEnemyAnimator component.");
        }
    }
    
    protected virtual void Start()
    {
        currentHealth = maxHealth;
        
        // Find player
        GameObject playerObj = GameObject.FindWithTag("Player");
        if (playerObj != null)
        {
            playerTransform = playerObj.transform;
        }
        else
        {
            Debug.LogWarning("No Player found with 'Player' tag.");
        }
        
        // Ensure enemy tag is set
        if (gameObject.tag != "Enemy")
        {
            Debug.LogWarning(gameObject.name + " doesn't have the 'Enemy' tag. Setting it automatically.");
            gameObject.tag = "Enemy";
        }
    }
    
    protected virtual void Update()
    {
        if (isDead || playerTransform == null)
            return;
        
        // Calculate distance to player
        distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);
        
        // Check if we can attack
        if (!isAttacking && distanceToPlayer <= attackRange && Time.time > nextAttackTime)
        {
            StartAttack();
        }
    }
    
    protected virtual void FixedUpdate()
    {
        if (isDead || playerTransform == null || rb == null || isAttacking) 
            return;
            
        MoveTowardsPlayer();
    }
    
    // Can be overridden by specific enemy types for different movement patterns
    protected virtual void MoveTowardsPlayer()
    {
        // Basic movement towards player
        Vector3 direction = playerTransform.position - transform.position;
        direction.y = 0; // Keep movement horizontal
        direction = direction.normalized;
        
        // Rotate towards player
        if (direction != Vector3.zero)
        {
            Quaternion lookRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * rotationSpeed);
        }
        
        // Move towards player
        rb.MovePosition(rb.position + direction * moveSpeed * Time.fixedDeltaTime);
    }
    
    protected virtual void StartAttack()
    {
        isAttacking = true;
        nextAttackTime = Time.time + attackCooldown;
        
        // Play attack animation
        if (simpleAnimator != null)
        {
            simpleAnimator.PlayAttackAnimation(attackDuration);
        }
        
        // Schedule end of attack state
        Invoke(nameof(FinishAttack), attackDuration);
    }
    
    protected virtual void FinishAttack()
    {
        isAttacking = false;
    }
    
    // This method is called from the animation event or controller
    public virtual void OnAttackHit()
    {
        if (isDead || playerTransform == null)
            return;
            
        // Only deal damage if player is in range and in front
        if (distanceToPlayer <= attackRange * 1.2f)
        {
            // Check if player is in front of the enemy
            Vector3 directionToPlayer = (playerTransform.position - transform.position).normalized;
            float dotProduct = Vector3.Dot(transform.forward, directionToPlayer);
            
            if (dotProduct > 0.5f) // Player is roughly in front of the enemy
            {
                PlayerStats playerStats = playerTransform.GetComponent<PlayerStats>();
                if (playerStats != null)
                {
                    playerStats.TakeDamage(damageToPlayer);
                }
            }
        }
    }
    
    // Take damage from player weapons
    public virtual void TakeDamageFromPlayer(float damageAmount, Vector3 hitDirection)
    {
        if (isDead)
            return;
            
        currentHealth -= damageAmount;
        
        // Play hurt animation
        if (simpleAnimator != null)
        {
            simpleAnimator.PlayHurtAnimation();
        }
        
        // Spawn damage effect
        ShowDamageEffect(hitDirection);
        
        if (currentHealth <= 0)
        {
            Die();
        }
    }
    
    // Take damage from special attacks
    public virtual void TakeDamage(float damageAmount)
    {
        if (isDead)
            return;
            
        currentHealth -= Mathf.RoundToInt(damageAmount);
        
        // Play hurt animation
        if (simpleAnimator != null)
        {
            simpleAnimator.PlayHurtAnimation();
        }
        
        // Spawn damage effect (no specific direction)
        ShowDamageEffect(Vector3.forward);
        
        if (currentHealth <= 0)
        {
            Die();
        }
    }
    
    // Show visual effects when taking damage
    protected virtual void ShowDamageEffect(Vector3 hitDirection)
    {
        if (damageEffectPrefab != null)
        {
            // Create effect at slightly offset position
            Vector3 effectPos = transform.position - hitDirection * 0.5f;
            effectPos.y = transform.position.y;
            
            Quaternion effectRotation = Quaternion.LookRotation(-hitDirection);
            GameObject effect = Instantiate(damageEffectPrefab, effectPos, effectRotation);
            Destroy(effect, 2f);
        }
    }
    
    // Enemy death
    protected virtual void Die()
    {
        isDead = true;
        
        // Stop movement and cancel any pending actions
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
        }
        
        CancelInvoke();
        
        // Play death animation
        if (simpleAnimator != null)
        {
            simpleAnimator.PlayDeathAnimation();
        }
        
        // Disable colliders
        Collider[] colliders = GetComponents<Collider>();
        foreach (Collider col in colliders)
        {
            col.enabled = false;
        }
        
        // Spawn soul
        if (soulPrefab != null)
        {
            Instantiate(soulPrefab, transform.position, Quaternion.identity);
        }
        
        // Destroy enemy after animation finishes
        Destroy(gameObject, 3f); // Adjust time based on your death animation length
    }
}