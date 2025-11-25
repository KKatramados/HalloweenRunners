using UnityEngine;

// FallingPlatform.cs - Attach to platform GameObjects
// Platform falls after player steps on it (like crumbling platforms)
public class FallingPlatform : MonoBehaviour
{
    [Header("Fall Settings")]
    public float fallDelay = 0.5f;
    public float shakeIntensity = 0.1f;
    public float shakeDuration = 0.3f;
    public float fallSpeed = 5f;
    
    [Header("Respawn Settings")]
    public bool respawnAfterFall = true;
    public float respawnDelay = 3f;
    
    [Header("Visual Feedback")]
    public Color warningColor = Color.red;
    
    private Rigidbody2D rb;
    private Vector3 originalPosition;
    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    private bool isFalling = false;
    private bool playerOnPlatform = false;
    private float fallTimer = 0f;
    
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        originalPosition = transform.position;
        
        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }
        
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
        }
        
        // Start as kinematic
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.gravityScale = 0f;
    }
    
    void Update()
    {
        if (playerOnPlatform && !isFalling)
        {
            fallTimer += Time.deltaTime;
            
            // Shake effect during fall delay
            if (fallTimer < fallDelay)
            {
                float shakeAmount = Mathf.Lerp(0f, shakeIntensity, fallTimer / shakeDuration);
                Vector3 shakeOffset = new Vector3(
                    Random.Range(-shakeAmount, shakeAmount),
                    Random.Range(-shakeAmount, shakeAmount),
                    0f
                );
                transform.position = originalPosition + shakeOffset;
                
                // Visual warning (color change)
                if (spriteRenderer != null)
                {
                    spriteRenderer.color = Color.Lerp(originalColor, warningColor, fallTimer / fallDelay);
                }
            }
            
            // Start falling after delay
            if (fallTimer >= fallDelay)
            {
                StartFalling();
            }
        }
    }
    
    void StartFalling()
    {
        isFalling = true;
        
        // Enable physics
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.gravityScale = fallSpeed;
        
        // Detach player
        Transform[] children = GetComponentsInChildren<Transform>();
        foreach (Transform child in children)
        {
            if (child != transform && child.CompareTag("Player"))
            {
                child.SetParent(null);
            }
        }
        
        // Respawn platform after falling
        if (respawnAfterFall)
        {
            Invoke(nameof(RespawnPlatform), respawnDelay);
        }
        else
        {
            Destroy(gameObject, 5f); // Destroy after falling off screen
        }
    }
    
    void RespawnPlatform()
    {
        // Reset position
        transform.position = originalPosition;
        
        // Reset physics
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.gravityScale = 0f;
        rb.linearVelocity = Vector2.zero;
        
        // Reset state
        isFalling = false;
        playerOnPlatform = false;
        fallTimer = 0f;
        
        // Reset color
        if (spriteRenderer != null)
        {
            spriteRenderer.color = originalColor;
        }
        
        // Re-enable collider
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
        {
            col.enabled = true;
        }
    }
    
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player") && !isFalling)
        {
            playerOnPlatform = true;
            collision.transform.SetParent(transform);
        }
    }
    
    void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            collision.transform.SetParent(null);
            
            // Stop timer if player leaves before platform falls
            if (!isFalling)
            {
                playerOnPlatform = false;
                fallTimer = 0f;
                
                // Reset visual warning
                if (spriteRenderer != null)
                {
                    spriteRenderer.color = originalColor;
                }
                
                // Reset position from shake
                transform.position = originalPosition;
            }
        }
    }
    
    void OnDrawGizmos()
    {
        // Visualize platform in editor
        Gizmos.color = isFalling ? Color.red : Color.yellow;
        
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
        {
            Gizmos.DrawWireCube(transform.position, col.bounds.size);
        }
        
        // Show original position if falling
        if (Application.isPlaying && isFalling)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(originalPosition, col != null ? col.bounds.size : Vector3.one);
        }
    }
}
