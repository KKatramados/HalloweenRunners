using UnityEngine;

// SkeletonBoss.cs - Attach to Skeleton Boss enemy
public class SkeletonBoss : MonoBehaviour
{
    [Header("Boss Stats")]
    public int maxHealth = 40;
    public int damage = 2; // Deals 2 half-hearts (1 full heart)
    public float moveSpeed = 2.5f;

    [Header("Attack Settings")]
    public float attackRange = 3f;
    public float attackCooldown = 2.5f;
    public float detectionRange = 10f;

    [Header("Room System")]
    public int roomNumber = 0; // Optional: for room progression

    private int currentHealth;
    private Animator animator;
    private Rigidbody2D rb;
    private Transform player;
    private float attackTimer = 0f;
    private bool isDead = false;
    private bool isAttacking = false;
    private int facingDirection = 1;
    private float damageTimer = 0f;

    // Animation hashes
    private int walkHash;
    private int attackHash;
    private int getHitHash;
    private int deathHash;

    void Start()
    {
        currentHealth = maxHealth;
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();

        // Find closest player
        FindClosestPlayer();

        // Cache animation hashes
        walkHash = Animator.StringToHash("Walk");
        attackHash = Animator.StringToHash("Attack");
        getHitHash = Animator.StringToHash("Get Hit");
        deathHash = Animator.StringToHash("Death Skeleton");

        // Register with room if assigned
        if (roomNumber > 0)
        {
            RoomManager room = FindFirstObjectByType<RoomManager>();
            if (room != null)
            {
                room.RegisterEnemy(roomNumber, this);
            }
        }

        // AUDIO: Play boss roar when skeleton boss appears
        if (AudioManager.instance != null)
        {
            AudioManager.instance.PlayBossRoar();
        }
    }

    void Update()
    {
        if (isDead || isAttacking) return;

        // Update attack timer
        if (attackTimer > 0)
            attackTimer -= Time.deltaTime;

        if (damageTimer > 0)
            damageTimer -= Time.deltaTime;

        // Find closest player if current target is invalid
        if (player == null || !player.gameObject.activeSelf)
        {
            FindClosestPlayer();
        }

        if (player == null) return;

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        // Face player
        if (player.position.x > transform.position.x && facingDirection < 0)
        {
            Flip();
        }
        else if (player.position.x < transform.position.x && facingDirection > 0)
        {
            Flip();
        }

        // AI Behavior
        if (distanceToPlayer <= attackRange && attackTimer <= 0)
        {
            // In attack range - attack
            StartCoroutine(AttackSequence());
        }
        else if (distanceToPlayer <= detectionRange)
        {
            // In detection range - move towards player
            MoveTowardsPlayer();
        }
        else
        {
            // Too far - stop moving
            StopMoving();
        }
    }

    void FindClosestPlayer()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        float closestDist = float.MaxValue;

        foreach (var p in players)
        {
            PlayerController pc = p.GetComponent<PlayerController>();
            if (pc != null && pc.IsActive())
            {
                float dist = Vector3.Distance(transform.position, p.transform.position);
                if (dist < closestDist)
                {
                    closestDist = dist;
                    player = p.transform;
                }
            }
        }
    }

    void MoveTowardsPlayer()
    {
        if (player == null) return;

        Vector2 direction = (player.position - transform.position).normalized;

        if (rb != null)
        {
            rb.linearVelocity = new Vector2(direction.x * moveSpeed, rb.linearVelocity.y);
        }

        PlayAnimation(walkHash);
    }

    void StopMoving()
    {
        if (rb != null)
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        }

        // Play idle or just stop walk animation
        // If you don't have an idle animation, the Walk animation will just stop
    }

    System.Collections.IEnumerator AttackSequence()
    {
        isAttacking = true;
        attackTimer = attackCooldown;

        // Stop moving
        if (rb != null)
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        }

        // Play attack animation
        PlayAnimation(attackHash);

        // Wait for attack animation to reach hit frame
        yield return new WaitForSeconds(0.4f);

        // Deal damage
        DealDamage();

        // Wait for attack animation to finish
        yield return new WaitForSeconds(0.4f);

        isAttacking = false;
    }

    void DealDamage()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, attackRange);
        foreach (var hit in hits)
        {
            if (hit.CompareTag("Player"))
            {
                PlayerController pc = hit.GetComponent<PlayerController>();
                if (pc != null && pc.IsActive())
                {
                    pc.TakeDamage(damage);
                }
            }
        }
    }

    void Flip()
    {
        facingDirection *= -1;
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }

    void PlayAnimation(int animHash)
    {
        if (animator != null)
        {
            animator.Play(animHash);
        }
    }

    public void TakeDamage(int amount)
    {
        if (isDead) return;

        currentHealth -= amount;

        // AUDIO: Play boss hit sound
        if (AudioManager.instance != null)
            AudioManager.instance.PlayBossHit();

        // Play hit animation
        if (!isAttacking)
        {
            PlayAnimation(getHitHash);
        }

        StartCoroutine(FlashRed());

        if (currentHealth <= 0)
        {
            Die();
        }
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

    void Die()
    {
        isDead = true;

        // Stop movement
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
        }

        if (AudioManager.instance != null)
            AudioManager.instance.PlayEnemyDeath();

        // Play death animation
        PlayAnimation(deathHash);

        // Award score
        GameManager.instance.AddScoreToAll(500);

        // Notify room manager if part of room system
        if (roomNumber > 0)
        {
            RoomManager room = FindFirstObjectByType<RoomManager>();
            if (room != null)
            {
                room.EnemyDied(roomNumber);
            }
        }

        // GameManager.instance.BossDefeated();
        // Destroy after animation plays
        // Destroy(gameObject, 3f);
    }

    void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player") && damageTimer <= 0 && !isDead)
        {
            PlayerController pc = collision.gameObject.GetComponent<PlayerController>();
            if (pc != null && pc.IsActive())
            {
                pc.TakeDamage(damage);
                damageTimer = 1f;
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        // Visualize attack range
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        // Visualize detection range
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
    }
}