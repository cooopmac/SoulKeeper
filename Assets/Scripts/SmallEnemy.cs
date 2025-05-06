using UnityEngine;

public class SmallEnemy : BaseEnemy
{
    [Header("Small Enemy Settings")]
    public float movementSpeed = 4.5f;  // Fast movement speed
    public float damageInterval = 0.5f; // Time between damage ticks (in seconds)
    
    private float damageTimer;          // Timer to track when to deal damage next
    private bool isTouchingPlayer;      // Flag to track if enemy is in contact with player
    
    protected override void Start()
    {
        base.Start();
        
        // Override base stats for small enemy
        maxHealth = 15;  // Low health
        currentHealth = maxHealth;
        damageToPlayer = 5;  // Low damage
        moveSpeed = movementSpeed;
        attackRange = 1.5f;   // Close attack range
        attackDuration = 0.5f; // Quick attack

        // Initialize damage timer
        damageTimer = 0f;
        isTouchingPlayer = false;
    }
    
    protected override void Update()
    {
        base.Update();
        
        // Update attack state based on touching player
        if (isTouchingPlayer && !isAttacking)
        {
            StartAttack();
        }
    }

    // Handle initial collision with player - damage on contact
    void OnCollisionEnter(Collision collision)
    {
        if (isDead) return;
            
        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerStats playerStats = collision.gameObject.GetComponent<PlayerStats>();
            if (playerStats != null)
            {
                Debug.Log("Small enemy hitting player for " + damageToPlayer + " damage");
                playerStats.TakeDamage(damageToPlayer);
                isTouchingPlayer = true; // Set flag when contact begins
                
                // Play attack animation
                if (simpleAnimator != null)
                {
                    simpleAnimator.PlayAttackAnimation(0.5f);
                }
            }
        }
    }

    // Handle continuous damage while in contact with player
    void OnCollisionStay(Collision collision)
    {
        if (isDead) return;
            
        if (collision.gameObject.CompareTag("Player") && isTouchingPlayer)
        {
            PlayerStats playerStats = collision.gameObject.GetComponent<PlayerStats>();
            if (playerStats != null)
            {
                // Update damage timer
                damageTimer -= Time.deltaTime;
                if (damageTimer <= 0f)
                {
                    Debug.Log("Small enemy dealing continuous damage: " + damageToPlayer);
                    playerStats.TakeDamage(damageToPlayer);
                    damageTimer = damageInterval; // Reset timer
                    
                    // Refresh attack animation
                    if (simpleAnimator != null)
                    {
                        simpleAnimator.PlayAttackAnimation(0.5f);
                    }
                }
            }
        }
    }

    // Stop continuous damage when contact ends
    void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            isTouchingPlayer = false; // Reset flag when contact ends
            damageTimer = 0f;         // Reset timer
            
            // Resume walking animation
            if (simpleAnimator != null && !isDead)
            {
                simpleAnimator.PlayWalkAnimation();
            }
        }
    }
    
    // Custom attack hit behavior
    public override void OnAttackHit()
    {
        // Already handled by collision logic for this enemy type
    }
}