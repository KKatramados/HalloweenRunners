using UnityEngine;

// Enemy.cs - Attach to enemy GameObjects
public class Enemy : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 2f;
    public float patrolDistance = 3f;
    public bool usePhysicsMovement = false;

    [Header("Obstacle Detection")]
    public bool detectObstacles = true;
    public Transform obstacleCheck; // Place this in front of enemy
    public float obstacleCheckDistance = 0.5f;
    public LayerMask obstacleLayer; // Set to Ground layer
    public bool detectGroundEdge = true;
    public Transform groundEdgeCheck; // Place this at enemy's feet, slightly forward
    public float groundEdgeCheckDistance = 0.5f;
    public LayerMask groundLayer;

    [Header("Combat")]
    public int health = 2;
    public int damage = 1; // Deals 1 half-heart of damage

    [Header("Room System")]
    public int roomNumber = 0; // 0 = not part of room system, 1-3 = room number

    [Header("Candy Drop System")]
    public bool isCandyEnemy = false;
    public GameObject[] candyPrefabs; // Array of 3 candy types (Red, Blue, Green)
    public int candiesPerHit = 5;

    private Rigidbody2D rb;
    private Vector3 startPos;
    private int direction = 1;
    private float damageTimer = 0f;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        startPos = transform.position;

        // Register with room if assigned
        if (roomNumber > 0)
        {
            RoomManager room = FindObjectOfType<RoomManager>();
            if (room != null)
            {
                room.RegisterEnemy(roomNumber, this);
            }
        }
    }

    void Update()
    {
        if (damageTimer > 0)
            damageTimer -= Time.deltaTime;
    }

    void FixedUpdate()
    {
        bool shouldFlip = false;
        
        // Check for obstacles ahead
        if (detectObstacles && obstacleCheck != null)
        {
            Vector2 rayOrigin = obstacleCheck.position;
            Vector2 rayDirection = direction > 0 ? Vector2.right : Vector2.left;
            
            RaycastHit2D obstacleHit = Physics2D.Raycast(rayOrigin, rayDirection, obstacleCheckDistance, obstacleLayer);
            
            if (obstacleHit.collider != null)
            {
                // Hit an obstacle - need to turn around
                shouldFlip = true;
                Debug.DrawRay(rayOrigin, rayDirection * obstacleCheckDistance, Color.red);
            }
            else
            {
                Debug.DrawRay(rayOrigin, rayDirection * obstacleCheckDistance, Color.green);
            }
        }
        
        // Check for ground edge (to prevent falling off platforms)
        if (detectGroundEdge && groundEdgeCheck != null && !shouldFlip)
        {
            Vector2 rayOrigin = groundEdgeCheck.position;
            
            RaycastHit2D groundHit = Physics2D.Raycast(rayOrigin, Vector2.down, groundEdgeCheckDistance, groundLayer);
            
            if (groundHit.collider == null)
            {
                // No ground ahead - need to turn around
                shouldFlip = true;
                Debug.DrawRay(rayOrigin, Vector2.down * groundEdgeCheckDistance, Color.red);
            }
            else
            {
                Debug.DrawRay(rayOrigin, Vector2.down * groundEdgeCheckDistance, Color.cyan);
            }
        }

        // Check patrol boundaries
        float distanceFromStart = transform.position.x - startPos.x;

        if (distanceFromStart >= patrolDistance && direction == 1)
        {
            shouldFlip = true;
        }
        else if (distanceFromStart <= -patrolDistance && direction == -1)
        {
            shouldFlip = true;
        }

        // Flip if needed
        if (shouldFlip)
        {
            direction *= -1;
            Flip();
        }

        // Move enemy
        if (usePhysicsMovement && rb != null)
        {
            rb.linearVelocity = new Vector2(direction * moveSpeed, rb.linearVelocity.y);
        }
        else
        {
            transform.position += new Vector3(direction * moveSpeed * Time.fixedDeltaTime, 0, 0);
        }
    }

    void Flip()
    {
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }

    public void TakeDamage(int amount)
    {
        health -= amount;

        // AUDIO: Play enemy hit sound
        if (AudioManager.instance != null)
            AudioManager.instance.PlayEnemyHit();

        StartCoroutine(FlashRed());

        if (isCandyEnemy && candyPrefabs != null && candyPrefabs.Length > 0)
        {
            SpawnCandyFountain();
        }

        if (health <= 0)
        {
            Die();
        }
    }

    void SpawnCandyFountain()
    {
        for (int i = 0; i < candiesPerHit; i++)
        {
            // Randomly select candy type from the array
            GameObject candyPrefab = candyPrefabs[Random.Range(0, candyPrefabs.Length)];
            if (candyPrefab == null) continue;

            GameObject candy = Instantiate(candyPrefab, transform.position, Quaternion.identity);

            // Add fountain-like physics to candy
            Rigidbody2D candyRb = candy.GetComponent<Rigidbody2D>();
            if (candyRb != null)
            {
                // Random angle between 45 and 135 degrees (upward arc)
                float angle = Random.Range(45f, 135f) * Mathf.Deg2Rad;
                float force = Random.Range(5f, 10f);

                Vector2 candyDirection = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
                candyRb.AddForce(candyDirection * force, ForceMode2D.Impulse);

                // Add random spin
                candyRb.angularVelocity = Random.Range(-360f, 360f);
            }
        }
    }

    void Die()
    {
        if (AudioManager.instance != null)
            AudioManager.instance.PlayEnemyDeath();

        GameManager.instance.AddScoreToAll(50);

        if (roomNumber > 0)
        {
            RoomManager room = FindObjectOfType<RoomManager>();
            if (room != null)
            {
                room.EnemyDied(roomNumber);
            }
        }

        Destroy(gameObject);
    }

    System.Collections.IEnumerator FlashRed()
    {
        SpriteRenderer[] spriteRenderers = GetComponentsInChildren<SpriteRenderer>();

        if (spriteRenderers.Length > 0)
        {
            Color[] originalColors = new Color[spriteRenderers.Length];
            for (int i = 0; i < spriteRenderers.Length; i++)
            {
                originalColors[i] = spriteRenderers[i].color;
                spriteRenderers[i].color = Color.red;
            }

            yield return new WaitForSeconds(0.1f);

            for (int i = 0; i < spriteRenderers.Length; i++)
            {
                if (spriteRenderers[i] != null)
                {
                    spriteRenderers[i].color = originalColors[i];
                }
            }
        }
    }

    void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player") && damageTimer <= 0)
        {
            PlayerController player = collision.gameObject.GetComponent<PlayerController>();
            if (player != null && player.IsActive())
            {
                player.TakeDamage(damage);
                damageTimer = 1f;
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        // Visualize obstacle detection ray
        if (obstacleCheck != null && detectObstacles)
        {
            Gizmos.color = Color.red;
            Vector3 rayDirection = direction > 0 ? Vector3.right : Vector3.left;
            Gizmos.DrawRay(obstacleCheck.position, rayDirection * obstacleCheckDistance);
            Gizmos.DrawWireSphere(obstacleCheck.position + rayDirection * obstacleCheckDistance, 0.1f);
        }

        // Visualize ground edge detection ray
        if (groundEdgeCheck != null && detectGroundEdge)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawRay(groundEdgeCheck.position, Vector3.down * groundEdgeCheckDistance);
            Gizmos.DrawWireSphere(groundEdgeCheck.position + Vector3.down * groundEdgeCheckDistance, 0.1f);
        }
    }
}