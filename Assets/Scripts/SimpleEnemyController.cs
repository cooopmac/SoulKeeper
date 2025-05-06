using UnityEngine;

public class SimpleEnemyController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 3.5f;
    public float rotationSpeed = 5f;
    
    [Header("Enemy Stats")]
    public int maxHealth = 30;
    public int currentHealth;
    public int damageToPlayer = 0;       // Damage this enemy does to player
    public int pointValue = 10;           // Score value when killed
    
    [Header("Effects")]
    public GameObject bloodEffectPrefab;
    
    [Header("References")]
    private Transform player;
    private Rigidbody rb;
    
    void Start()
    {
        // Get components
        rb = GetComponent<Rigidbody>();
        currentHealth = maxHealth;
        
        // Find player
        GameObject playerObj = GameObject.FindWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }
        
        // Make sure this object has the "Enemy" tag
        if (gameObject.tag != "Enemy")
        {
            Debug.LogWarning("This enemy doesn't have the 'Enemy' tag. The player's special attack won't detect it.");
        }
    }
    
    void FixedUpdate()
    {
        if (player == null || rb == null)
            return;
        
        // Calculate direction to player
        Vector3 direction = player.position - transform.position;
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
    
    // Method for player's weapons to damage this enemy (independent of special attack)
    public void TakeDamageFromPlayer(int damageAmount, Vector3 hitDirection)
    {
        currentHealth -= damageAmount;
        
        // Spawn blood at hit location
        if (bloodEffectPrefab != null)
        {
            // Calculate hit position (slightly offset from center)
            Vector3 hitPos = transform.position - hitDirection * 0.5f;
            hitPos.y = transform.position.y; // Keep at same height as enemy
            
            // Create blood effect facing away from hit direction
            Quaternion bloodRotation = Quaternion.LookRotation(-hitDirection);
            GameObject instantiatedBlood = Instantiate(bloodEffectPrefab, hitPos, bloodRotation);
            Destroy(instantiatedBlood, 2f);
        }
        
        if (currentHealth <= 0)
        {
            Die();
        }
    }
    
    // Method for player's special attack to damage this enemy
    public void TakeDamage(float damageAmount)
    {
        currentHealth -= Mathf.RoundToInt(damageAmount);
        
        // Spawn blood at center of enemy (simplified for special attack)
        if (bloodEffectPrefab != null)
        {
            Vector3 bloodPos = transform.position;
            Quaternion randomRot = Quaternion.Euler(0, Random.Range(0, 360), 0);
            GameObject instantiatedBlood = Instantiate(bloodEffectPrefab, bloodPos, randomRot);
            Destroy(instantiatedBlood, 2f);
        }
        
        if (currentHealth <= 0)
        {
            Die();
        }
    }
    
    void Die()
    {
        // Optional: spawn large blood effect on death
        if (bloodEffectPrefab != null)
        {
            // Create multiple blood splatters for death
            for (int i = 0; i < 3; i++)
            {
                Vector3 randomOffset = new Vector3(Random.Range(-0.5f, 0.5f), 0, Random.Range(-0.5f, 0.5f));
                Vector3 bloodPos = transform.position + randomOffset;
                Quaternion randomRot = Quaternion.Euler(0, Random.Range(0, 360), 0);
                GameObject instantiatedBlood = Instantiate(bloodEffectPrefab, bloodPos, randomRot);
                Destroy(instantiatedBlood, 1f);
            }
        }
        
        // Add score (commented out for now)
        //GameManager.Instance?.AddScore(pointValue);
        
        // Destroy this enemy
        Destroy(gameObject);
    }
    
    void OnCollisionEnter(Collision collision)
    {
        // Check if we hit the player
        if (collision.gameObject.CompareTag("Player"))
        {
            // Deal damage to player
            PlayerStats playerStats = collision.gameObject.GetComponent<PlayerStats>();
            if (playerStats != null)
            {
                Debug.Log("Damage to player: " + damageToPlayer);
                playerStats.TakeDamage(damageToPlayer);
            }
        }
    }
}
