using UnityEngine;

public class ShadowBolt : MonoBehaviour
{
    public float speed = 20f;
    public float lifetime = 2f;
    public float damage = 10;
    public float impactForce = 10f;
    
    private Vector3 shootDirection;
    
    void Start()
    {
        // Destroy after lifetime
        Destroy(gameObject, lifetime);
    }
    
    public void SetDirection(Vector3 direction)
    {
        shootDirection = direction.normalized;
        
        // Set initial velocity
        GetComponent<Rigidbody>().linearVelocity = shootDirection * speed;
        
        // Rotate to face direction
        transform.forward = shootDirection;
    }
    
    void OnCollisionEnter(Collision collision) // or OnTriggerEnter for triggers
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
            BaseEnemy enemy = collision.gameObject.GetComponent<BaseEnemy>();
            if (enemy != null)
            {
                // Calculate hit direction
                Vector3 hitDirection = (collision.transform.position - transform.position).normalized;
                
                // Apply damage
                enemy.TakeDamageFromPlayer(damage, hitDirection);
                Debug.Log("Damage applied to enemy: " + damage);
                Destroy(gameObject);
            }
        }
    }
}
