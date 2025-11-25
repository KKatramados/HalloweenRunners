using UnityEngine;

// DeathPit.cs - Attach to pit/hazard GameObjects
// Instantly kills or damages player on contact
public class DeathPit : MonoBehaviour
{
    [Header("Pit Settings")]
    public bool instantKill = true;
    public int damageAmount = 6; // Used if not instant kill
    
    [Header("Visual Effects")]
    public bool destroyOnContact = false;
    public GameObject deathEffectPrefab; // Optional particle effect
    public Color pitColor = new Color(0.1f, 0.1f, 0.1f, 1f);
    
    [Header("Respawn Settings")]
    public bool forceRespawn = true;
    public Transform respawnPoint; // Optional custom respawn location
    
    void Start()
    {
        // Make sure the pit has a trigger collider
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
        {
            col.isTrigger = true;
        }
        else
        {
            // Add a BoxCollider2D if none exists
            BoxCollider2D boxCol = gameObject.AddComponent<BoxCollider2D>();
            boxCol.isTrigger = true;
        }
        
        // Optional: Apply visual style
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            sr.color = pitColor;
        }
    }
    
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerController player = other.GetComponent<PlayerController>();
            
            if (player != null && player.IsActive() && !player.IsDead())
            {
                HandlePlayerFall(player);
            }
        }
        else if (other.CompareTag("Enemy"))
        {
            // Optionally handle enemies falling in pits
            HandleEnemyFall(other.gameObject);
        }
    }
    
    void HandlePlayerFall(PlayerController player)
    {
        // Spawn death effect
        if (deathEffectPrefab != null)
        {
            Instantiate(deathEffectPrefab, player.transform.position, Quaternion.identity);
        }
        
        if (instantKill)
        {
            // Deal massive damage to instantly kill
            player.TakeDamage(999);
        }
        else
        {
            // Deal specified damage
            player.TakeDamage(damageAmount);
        }
        
        // Optional: Force immediate respawn at specific location
        if (forceRespawn && respawnPoint != null && !player.IsDead())
        {
            player.transform.position = respawnPoint.position;
        }
        
        // AUDIO: Play death sound (handled by PlayerController)
    }
    
    void HandleEnemyFall(GameObject enemy)
    {
        // Spawn effect
        if (deathEffectPrefab != null)
        {
            Instantiate(deathEffectPrefab, enemy.transform.position, Quaternion.identity);
        }
        
        // Destroy enemy immediately (no score)
        Destroy(enemy);
    }
    
    void OnDrawGizmos()
    {
        // Visualize pit in editor
        Gizmos.color = Color.red;
        
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
        {
            Gizmos.DrawWireCube(transform.position, col.bounds.size);
        }
        
        // Draw respawn point if assigned
        if (respawnPoint != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(respawnPoint.position, 0.5f);
            Gizmos.DrawLine(transform.position, respawnPoint.position);
        }
    }
}
