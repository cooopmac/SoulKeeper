using UnityEngine;

public class BigEnemy : BaseEnemy
{
    [Header("Big Enemy Settings")]
    public float slowMoveSpeed = 1.8f;       // Slow movement speed
    public int highDamage = 50;              // High damage on collision
    public float bigEnemyAttackCooldown = 3f; // Longer cooldown between attacks
    
    private bool isTouchingPlayer;           // Flag to track if enemy is in contact with player
    
    protected override void Start()
    {
        base.Start();
        
        // Override base stats for big enemy
        maxHealth = 80;  // High health
        currentHealth = maxHealth;
        damageToPlayer = highDamage;  // High damage
        moveSpeed = slowMoveSpeed;    // Slow movement
        attackRange = 3f;             // Bigger attack range
        attackCooldown = bigEnemyAttackCooldown; // Longer attack cooldown
        attackDuration = 1.5f;        // Longer attack animation
    }
    
    protected override void Update()
    {
        base.Update();
        
        // Update attack based on touching player
        if (isTouchingPlayer && !isAttacking)
        {
            StartAttack();
        }
    }
    
    protected override void StartAttack()
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
        
        // Schedule damage application
        Invoke(nameof(ApplyAttackDamage), attackDuration * 0.5f);
    }
    
    private void ApplyAttackDamage()
    {
        if (isDead) return;
        
        OnAttackHit();
    }
    
    public override void OnAttackHit()
    {
        // Big enemy deals damage in a wider area
        if (playerTransform != null && Vector3.Distance(transform.position, playerTransform.position) < attackRange + 1f)
        {
            // More forgiving angle check for big enemy
            Vector3 directionToPlayer = (playerTransform.position - transform.position).normalized;
            float dotProduct = Vector3.Dot(transform.forward, directionToPlayer);
            
            if (dotProduct > 0.3f) // Player is roughly in front of the enemy with wider angle
            {
                PlayerStats playerStats = playerTransform.GetComponent<PlayerStats>();
                if (playerStats != null)
                {
                    Debug.Log("Big enemy attacking player for " + damageToPlayer + " damage");
                    playerStats.TakeDamage(damageToPlayer);
                    
                    // Add knockback effect
                    Rigidbody playerRb = playerTransform.GetComponent<Rigidbody>();
                    if (playerRb != null)
                    {
                        Vector3 knockbackDir = (playerTransform.position - transform.position).normalized;
                        playerRb.AddForce(knockbackDir * 8f, ForceMode.Impulse);
                    }
                }
            }
        }
    }
    
    // Handle collision with player - heavy damage on contact
    void OnCollisionEnter(Collision collision)
    {
        if (isDead) return;
            
        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerStats playerStats = collision.gameObject.GetComponent<PlayerStats>();
            if (playerStats != null)
            {
                // Reduce collision damage if we're in attack mode already
                int collisionDamage = isAttacking ? damageToPlayer / 2 : damageToPlayer;
                
                Debug.Log("Big enemy hitting player on collision for " + collisionDamage + " damage");
                playerStats.TakeDamage(collisionDamage);
                
                // Set touching flag
                isTouchingPlayer = true;
                
                // Add knockback effect
                Rigidbody playerRb = collision.gameObject.GetComponent<Rigidbody>();
                if (playerRb != null)
                {
                    Vector3 knockbackDir = (collision.transform.position - transform.position).normalized;
                    playerRb.AddForce(knockbackDir * 8f, ForceMode.Impulse);
                }
                
                // Play attack animation if not already attacking
                if (!isAttacking)
                {
                    StartAttack();
                }
            }
        }
    }
    
    void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            isTouchingPlayer = false;
            
            // Resume walking animation if not attacking
            if (!isAttacking && !isDead && simpleAnimator != null)
            {
                simpleAnimator.PlayWalkAnimation();
            }
        }
    }
    
    // Big enemies take less damage from special attacks
    public override void TakeDamage(float damageAmount)
    {
        // Reduce incoming damage by 25%
        float reducedDamage = damageAmount * 0.75f;
        base.TakeDamage(reducedDamage);
    }
    
    protected override void Die()
    {
        isDead = true;
        
        // Stop all movement and behaviors
        isTouchingPlayer = false;
        isAttacking = false;
        CancelInvoke();
        
        // Specific big enemy death effect before calling base implementation
        if (deathEffectPrefab != null)
        {
            GameObject effect = Instantiate(deathEffectPrefab, transform.position, Quaternion.identity);
            effect.transform.localScale = new Vector3(1.5f, 1.5f, 1.5f); // Bigger effect
            Destroy(effect, 3f);
        }
        
        // Call base implementation for common death behavior
        base.Die();
    }
}