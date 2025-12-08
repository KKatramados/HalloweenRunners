using UnityEngine;

// Enemy.cs - Unified enemy system with multiple AI modes
// Supports: Patrol, Chase, Archer, Caster, Melee behaviors
// Attach to enemy GameObjects
public class Enemy : MonoBehaviour
{
    [Header("Enemy Type")]
    public EnemyMode enemyMode = EnemyMode.Melee;
    
    [Header("Movement")]
    public float moveSpeed = 2f;
    public float patrolDistance = 3f; // Only used in Patrol mode
    public bool usePhysicsMovement = true;

    [Header("AI Behavior")]
    public float detectionRange = 8f; // Distance at which enemy detects player
    public float attackRange = 1.5f; // Distance for melee attacks
    public float rangedAttackRange = 6f; // Distance for ranged attacks (Archer/Caster)
    public float attackCooldown = 2f; // Time between attacks
    public float chaseSpeed = 3f; // Speed when chasing player (Chase/Melee modes)

    [Header("Ranged Attack Settings")]
    public GameObject projectilePrefab; // For Archer/Caster
    public Transform projectileSpawnPoint; // Where projectiles spawn
    public float projectileSpeed = 8f;
    public int projectileDamage = 1;

    [Header("Obstacle Detection")]
    public bool detectObstacles = true;
    public Transform obstacleCheck;
    public float obstacleCheckDistance = 0.5f;
    public LayerMask obstacleLayer;
    public bool detectGroundEdge = true;
    public Transform groundEdgeCheck;
    public float groundEdgeCheckDistance = 0.5f;
    public LayerMask groundLayer;

    [Header("Combat")]
    public int health = 3;
    public int meleeDamage = 1; // Damage for melee/collision attacks
    public bool canMoveWhileAttacking = false; // Casters might cast while moving

    [Header("Room System")]
    public int roomNumber = 0; // 0 = not part of room system

    [Header("Candy Drop System")]
    public bool isCandyEnemy = false;
    public GameObject[] candyPrefabs;
    public int candiesPerHit = 5;

    [Header("Death Effect")]
    public GameObject smokeEffectPrefab;
    public float deathAnimationDuration = 1f;

    [Header("Ground Check")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;

    // Private variables
    private Rigidbody2D rb;
    private Animator animator;
    private Vector3 startPosition;
    private Transform targetPlayer;
    private float attackTimer = 0f;
    private float damageTimer = 0f;
    private int patrolDirection = 1; // 1 = right, -1 = left
    private int facingDirection = 1;
    
    // State flags
    private EnemyState currentState = EnemyState.Idle;
    private bool isAttacking = false;
    private bool isDead = false;
    private bool isGrounded = false;
    private bool isCasting = false;
    
    // Animation hashes
    private int idleHash;
    private int walkHash;
    private int runHash;
    private int attackHash;
    private int rangedAttackHash;
    private int castHash;
    private int hurtHash;
    private int dieHash;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        startPosition = transform.position;

        // Cache animation hashes
        idleHash = Animator.StringToHash("Idle");
        walkHash = Animator.StringToHash("Walk");
        runHash = Animator.StringToHash("Run");
        attackHash = Animator.StringToHash("Attack");
        rangedAttackHash = Animator.StringToHash("RangedAttack");
        castHash = Animator.StringToHash("Cast");
        hurtHash = Animator.StringToHash("Hurt");
        dieHash = Animator.StringToHash("Die");

        // Set initial state based on mode
        switch (enemyMode)
        {
            case EnemyMode.Patrol:
            case EnemyMode.Archer:
            case EnemyMode.Caster:
                currentState = EnemyState.Patrolling;
                break;
            case EnemyMode.Chase:
            case EnemyMode.Melee:
                currentState = EnemyState.Idle;
                break;
        }

        // Register with room if assigned
        if (roomNumber > 0)
        {
            RoomManager room = FindFirstObjectByType<RoomManager>();
            if (room != null)
            {
                room.RegisterEnemy(roomNumber, this);
            }
        }
    }

    void Update()
    {
        if (isDead) return;

        // Update timers
        if (attackTimer > 0)
            attackTimer -= Time.deltaTime;

        if (damageTimer > 0)
            damageTimer -= Time.deltaTime;

        // Ground check
        if (groundCheck != null)
        {
            isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
        }

        // Update target player
        UpdateTargetPlayer();

        // Run AI based on current mode
        switch (enemyMode)
        {
            case EnemyMode.Patrol:
                UpdatePatrolMode();
                break;
            case EnemyMode.Chase:
                UpdateChaseMode();
                break;
            case EnemyMode.Archer:
                UpdateArcherMode();
                break;
            case EnemyMode.Caster:
                UpdateCasterMode();
                break;
            case EnemyMode.Melee:
                UpdateMeleeMode();
                break;
        }

        // Update animations
        UpdateAnimations();
    }

    void FixedUpdate()
    {
        if (isDead) return;

        // Handle movement
        if (!isAttacking || canMoveWhileAttacking)
        {
            HandleMovement();
        }
        else
        {
            StopMovement();
        }
    }

    #region AI Mode Updates

    void UpdatePatrolMode()
    {
        // Patrol mode: walks back and forth, attacks when player is close
        
        if (targetPlayer != null)
        {
            float distanceToPlayer = Vector3.Distance(transform.position, targetPlayer.position);

            if (distanceToPlayer <= attackRange && !isAttacking && attackTimer <= 0)
            {
                currentState = EnemyState.Attacking;
                FaceTarget(targetPlayer.position);
                StartCoroutine(MeleeAttackSequence());
                return;
            }
        }

        currentState = EnemyState.Patrolling;
    }

    void UpdateChaseMode()
    {
        // Chase mode: stands idle until player detected, then chases
        
        if (targetPlayer == null)
        {
            currentState = EnemyState.Idle;
            return;
        }

        float distanceToPlayer = Vector3.Distance(transform.position, targetPlayer.position);

        if (distanceToPlayer > detectionRange)
        {
            currentState = EnemyState.Idle;
            return;
        }

        FaceTarget(targetPlayer.position);

        if (distanceToPlayer <= attackRange && !isAttacking && attackTimer <= 0)
        {
            currentState = EnemyState.Attacking;
            StartCoroutine(MeleeAttackSequence());
        }
        else
        {
            currentState = EnemyState.Chasing;
        }
    }

    void UpdateArcherMode()
    {
        // Archer mode: patrols, keeps distance, shoots projectiles
        
        if (targetPlayer == null)
        {
            currentState = EnemyState.Patrolling;
            return;
        }

        float distanceToPlayer = Vector3.Distance(transform.position, targetPlayer.position);

        if (distanceToPlayer > detectionRange)
        {
            currentState = EnemyState.Patrolling;
            return;
        }

        FaceTarget(targetPlayer.position);

        // Keep optimal distance
        if (distanceToPlayer <= attackRange)
        {
            // Too close - back away
            currentState = EnemyState.Retreating;
        }
        else if (distanceToPlayer <= rangedAttackRange && !isAttacking && attackTimer <= 0)
        {
            // In range - shoot
            currentState = EnemyState.Attacking;
            StartCoroutine(RangedAttackSequence());
        }
        else if (distanceToPlayer > rangedAttackRange)
        {
            // Too far - move closer
            currentState = EnemyState.Chasing;
        }
        else
        {
            currentState = EnemyState.Idle;
        }
    }

    void UpdateCasterMode()
    {
        // Caster mode: patrols, can cast while moving, uses magic projectiles
        
        if (targetPlayer == null)
        {
            currentState = EnemyState.Patrolling;
            isCasting = false;
            return;
        }

        float distanceToPlayer = Vector3.Distance(transform.position, targetPlayer.position);

        if (distanceToPlayer > detectionRange)
        {
            currentState = EnemyState.Patrolling;
            isCasting = false;
            return;
        }

        FaceTarget(targetPlayer.position);

        if (distanceToPlayer <= rangedAttackRange && !isAttacking && attackTimer <= 0)
        {
            currentState = EnemyState.Attacking;
            isCasting = true;
            StartCoroutine(CastAttackSequence());
        }
        else if (distanceToPlayer > rangedAttackRange)
        {
            currentState = EnemyState.Chasing;
            isCasting = false;
        }
        else
        {
            currentState = EnemyState.Idle;
            isCasting = false;
        }
    }

    void UpdateMeleeMode()
    {
        // Melee mode: aggressive chase and attack
        
        if (targetPlayer == null)
        {
            currentState = EnemyState.Idle;
            return;
        }

        float distanceToPlayer = Vector3.Distance(transform.position, targetPlayer.position);

        if (distanceToPlayer > detectionRange)
        {
            currentState = EnemyState.Idle;
            return;
        }

        FaceTarget(targetPlayer.position);

        if (distanceToPlayer <= attackRange && !isAttacking && attackTimer <= 0)
        {
            currentState = EnemyState.Attacking;
            StartCoroutine(MeleeAttackSequence());
        }
        else
        {
            currentState = EnemyState.Chasing;
        }
    }

    #endregion

    #region Movement Handling

    void HandleMovement()
    {
        Vector2 velocity = Vector2.zero;

        switch (currentState)
        {
            case EnemyState.Idle:
                velocity = Vector2.zero;
                break;

            case EnemyState.Patrolling:
                velocity = new Vector2(patrolDirection * moveSpeed, 0);
                HandlePatrolBoundaries();
                break;

            case EnemyState.Chasing:
                if (targetPlayer != null)
                {
                    float speed = (enemyMode == EnemyMode.Melee || enemyMode == EnemyMode.Chase) ? chaseSpeed : moveSpeed;
                    Vector2 direction = (targetPlayer.position - transform.position).normalized;
                    velocity = new Vector2(direction.x * speed, 0);
                }
                break;

            case EnemyState.Retreating:
                if (targetPlayer != null)
                {
                    Vector2 direction = (transform.position - targetPlayer.position).normalized;
                    velocity = new Vector2(direction.x * moveSpeed * 0.7f, 0);
                }
                break;

            case EnemyState.Attacking:
                if (canMoveWhileAttacking && targetPlayer != null)
                {
                    Vector2 direction = (targetPlayer.position - transform.position).normalized;
                    velocity = new Vector2(direction.x * moveSpeed * 0.5f, 0);
                }
                else
                {
                    velocity = Vector2.zero;
                }
                break;
        }

        // Apply velocity
        if (rb != null && usePhysicsMovement)
        {
            rb.linearVelocity = new Vector2(velocity.x, rb.linearVelocity.y);
        }
        else if (!usePhysicsMovement)
        {
            transform.position += new Vector3(velocity.x * Time.fixedDeltaTime, 0, 0);
        }
    }

    void StopMovement()
    {
        if (rb != null)
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        }
    }

    void HandlePatrolBoundaries()
    {
        bool shouldFlip = false;

        // Check for obstacles
        if (detectObstacles && obstacleCheck != null)
        {
            Vector2 rayOrigin = obstacleCheck.position;
            Vector2 rayDirection = patrolDirection > 0 ? Vector2.right : Vector2.left;
            RaycastHit2D obstacleHit = Physics2D.Raycast(rayOrigin, rayDirection, obstacleCheckDistance, obstacleLayer);

            if (obstacleHit.collider != null)
                shouldFlip = true;
        }

        // Check for ground edge
        if (detectGroundEdge && groundEdgeCheck != null && !shouldFlip)
        {
            Vector2 rayOrigin = groundEdgeCheck.position;
            RaycastHit2D groundHit = Physics2D.Raycast(rayOrigin, Vector2.down, groundEdgeCheckDistance, groundLayer);

            if (groundHit.collider == null)
                shouldFlip = true;
        }

        // Check patrol distance
        float distanceFromStart = transform.position.x - startPosition.x;
        if (distanceFromStart >= patrolDistance && patrolDirection == 1)
            shouldFlip = true;
        else if (distanceFromStart <= -patrolDistance && patrolDirection == -1)
            shouldFlip = true;

        if (shouldFlip)
        {
            patrolDirection *= -1;
            FlipSprite(patrolDirection);
        }
    }

    #endregion

    #region Attack Sequences

    System.Collections.IEnumerator MeleeAttackSequence()
    {
        isAttacking = true;
        attackTimer = attackCooldown;

        StopMovement();
        PlayAnimation(attackHash);

        // Wait for attack animation to reach hit frame
        yield return new WaitForSeconds(1f);

        // Deal damage
        DealMeleeDamageToNearbyPlayers();

        // Wait for animation to finish
        yield return new WaitForSeconds(0.3f);

        isAttacking = false;
    }

    System.Collections.IEnumerator RangedAttackSequence()
    {
        isAttacking = true;
        attackTimer = attackCooldown;

        StopMovement();
        PlayAnimation(rangedAttackHash);

        // Wait for animation to reach shoot frame
        yield return new WaitForSeconds(0.4f);

        // Fire projectile
        FireProjectile();

        // Wait for animation to finish
        yield return new WaitForSeconds(0.3f);

        isAttacking = false;
    }

    System.Collections.IEnumerator CastAttackSequence()
    {
        isAttacking = true;
        attackTimer = attackCooldown;

        PlayAnimation(castHash);

        // Wait for cast animation
        yield return new WaitForSeconds(0.5f);

        // Fire magical projectile
        FireProjectile();

        yield return new WaitForSeconds(0.2f);

        isAttacking = false;
        isCasting = false;
    }

    void DealMeleeDamageToNearbyPlayers()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, attackRange);

        foreach (var hit in hits)
        {
            if (hit.CompareTag("Player"))
            {
                PlayerController pc = hit.GetComponent<PlayerController>();
                if (pc != null && pc.IsActive() && !pc.IsDead())
                {
                    pc.TakeDamage(meleeDamage);
                }
            }
        }
    }

    void FireProjectile()
    {
        if (projectilePrefab == null || targetPlayer == null)
            return;

        Vector3 spawnPos = projectileSpawnPoint != null ? projectileSpawnPoint.position : transform.position;
        GameObject projectile = Instantiate(projectilePrefab, spawnPos, Quaternion.identity);

        // Set projectile direction
        Vector2 direction = (targetPlayer.position - spawnPos).normalized;
        
        Rigidbody2D projRb = projectile.GetComponent<Rigidbody2D>();
        if (projRb != null)
        {
            projRb.linearVelocity = direction * projectileSpeed;
        }

        // Set projectile damage
        EnemyProjectile projScript = projectile.GetComponent<EnemyProjectile>();
        if (projScript != null)
        {
            projScript.damage = projectileDamage;
        }
        else
        {
            // Add component if not present
            projScript = projectile.AddComponent<EnemyProjectile>();
            projScript.damage = projectileDamage;
        }

        // Destroy projectile after lifetime
        Destroy(projectile, 5f);

        // AUDIO
        if (AudioManager.instance != null)
            AudioManager.instance.PlayProjectile();
    }

    #endregion

    #region Helper Methods

    void UpdateTargetPlayer()
    {
        // Check if current target is still valid
        if (targetPlayer != null)
        {
            PlayerController pc = targetPlayer.GetComponent<PlayerController>();
            if (pc == null || !pc.IsActive() || pc.IsDead())
            {
                targetPlayer = null;
            }
        }

        // Find closest player within detection range
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        float closestDistance = float.MaxValue;
        Transform closestPlayer = null;

        foreach (var player in players)
        {
            PlayerController pc = player.GetComponent<PlayerController>();
            if (pc != null && pc.IsActive() && !pc.IsDead())
            {
                float distance = Vector3.Distance(transform.position, player.transform.position);

                if (distance <= detectionRange && distance < closestDistance)
                {
                    closestDistance = distance;
                    closestPlayer = player.transform;
                }
            }
        }

        targetPlayer = closestPlayer;
    }

    void FaceTarget(Vector3 targetPosition)
    {
        int targetDirection = targetPosition.x > transform.position.x ? 1 : -1;
        
        if (targetDirection != facingDirection)
        {
            FlipSprite(targetDirection);
        }
    }

    void FlipSprite(int direction)
    {
        facingDirection = direction;
        Vector3 scale = transform.localScale;
        scale.x = Mathf.Abs(scale.x) * direction;
        transform.localScale = scale;
    }

    void PlayAnimation(int animHash)
    {
        if (animator != null && animator.HasState(0, animHash))
        {
            animator.Play(animHash);
        }
    }

    void UpdateAnimations()
    {
        if (animator == null || isDead || isAttacking) return;

        switch (currentState)
        {
            case EnemyState.Idle:
                PlayAnimation(idleHash);
                break;

            case EnemyState.Patrolling:
                PlayAnimation(walkHash);
                break;

            case EnemyState.Chasing:
                PlayAnimation(walkHash);
                // if (enemyMode == EnemyMode.Melee || enemyMode == EnemyMode.Chase)
                // {

                //     PlayAnimation(runHash);
                // }
                // else
                // {
                //     PlayAnimation(walkHash);
                // }
                break;

            case EnemyState.Retreating:
                PlayAnimation(walkHash);
                break;
        }
    }

    #endregion

    #region Damage and Death

    public void TakeDamage(int amount)
    {
        if (isDead) return;

        health -= amount;

        // AUDIO
        if (AudioManager.instance != null)
            AudioManager.instance.PlayEnemyHit();

        StartCoroutine(FlashRed());

        // Spawn candy if candy enemy
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
            GameObject candyPrefab = candyPrefabs[Random.Range(0, candyPrefabs.Length)];
            if (candyPrefab == null) continue;

            GameObject candy = Instantiate(candyPrefab, transform.position, Quaternion.identity);

            Rigidbody2D candyRb = candy.GetComponent<Rigidbody2D>();
            if (candyRb != null)
            {
                float angle = Random.Range(45f, 135f) * Mathf.Deg2Rad;
                float force = Random.Range(5f, 10f);

                Vector2 candyDirection = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
                candyRb.AddForce(candyDirection * force, ForceMode2D.Impulse);
                candyRb.angularVelocity = Random.Range(-360f, 360f);
            }
        }
    }

    void Die()
    {
        if (isDead) return;

        isDead = true;
        currentState = EnemyState.Dead;

        StopMovement();

        // if (rb != null)
        //     rb.simulated = false;

        Collider2D[] colliders = GetComponents<Collider2D>();
        foreach (var col in colliders)
            col.enabled = false;

        // AUDIO
        if (AudioManager.instance != null)
            AudioManager.instance.PlayEnemyDeath();

        // Add score
        if (GameManager.instance != null)
            GameManager.instance.AddScoreToAll(50);

        // Notify room manager
        if (roomNumber > 0)
        {
            RoomManager room = FindFirstObjectByType<RoomManager>();
            if (room != null)
                room.EnemyDied(roomNumber);
        }

        StartCoroutine(DeathSequence());
    }

    System.Collections.IEnumerator DeathSequence()
    {
        PlayAnimation(dieHash);

        yield return new WaitForSeconds(deathAnimationDuration);

        // Spawn smoke effect
        if (smokeEffectPrefab != null)
        {
            GameObject smoke = Instantiate(smokeEffectPrefab, transform.position, Quaternion.identity);
            Destroy(smoke, 2f);
        }

        // Fade out
        SpriteRenderer[] spriteRenderers = GetComponentsInChildren<SpriteRenderer>();
        float fadeDuration = 0.5f;
        float elapsed = 0f;

        Color[] originalColors = new Color[spriteRenderers.Length];
        for (int i = 0; i < spriteRenderers.Length; i++)
        {
            originalColors[i] = spriteRenderers[i].color;
        }

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsed / fadeDuration);

            for (int i = 0; i < spriteRenderers.Length; i++)
            {
                if (spriteRenderers[i] != null)
                {
                    Color color = originalColors[i];
                    color.a = alpha;
                    spriteRenderers[i].color = color;
                }
            }

            yield return null;
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

    #endregion

    #region Collision Damage

    void OnCollisionStay2D(Collision2D collision)
    {
        if (isDead) return;

        if (collision.gameObject.CompareTag("Player") && damageTimer <= 0)
        {
            PlayerController player = collision.gameObject.GetComponent<PlayerController>();
            if (player != null && player.IsActive() && !player.IsDead())
            {
                player.TakeDamage(meleeDamage);
                damageTimer = 1f;
            }
        }
    }

    #endregion

    #region Gizmos

    void OnDrawGizmosSelected()
    {
        // Detection range
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        // Attack range
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        // Ranged attack range
        if (enemyMode == EnemyMode.Archer || enemyMode == EnemyMode.Caster)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(transform.position, rangedAttackRange);
        }

        // Patrol distance
        if (enemyMode == EnemyMode.Patrol || enemyMode == EnemyMode.Archer || enemyMode == EnemyMode.Caster)
        {
            Gizmos.color = Color.green;
            Vector3 startPos = Application.isPlaying ? startPosition : transform.position;
            Gizmos.DrawLine(startPos + Vector3.left * patrolDistance, startPos + Vector3.right * patrolDistance);
        }

        // Ground check
        if (groundCheck != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }

        // Target line
        if (Application.isPlaying && targetPlayer != null)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawLine(transform.position, targetPlayer.position);
        }

        // Obstacle check
        if (obstacleCheck != null && detectObstacles)
        {
            Gizmos.color = Color.red;
            Vector3 rayDir = (Application.isPlaying ? patrolDirection : 1) > 0 ? Vector3.right : Vector3.left;
            Gizmos.DrawRay(obstacleCheck.position, rayDir * obstacleCheckDistance);
        }

        // Ground edge check
        if (groundEdgeCheck != null && detectGroundEdge)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawRay(groundEdgeCheck.position, Vector3.down * groundEdgeCheckDistance);
        }
    }

    #endregion
}

#region Enemy Enums

public enum EnemyMode
{
    Patrol,  // Walks back and forth, attacks when player is close
    Chase,   // Stands idle until player detected, then chases
    Archer,  // Patrols, keeps distance, shoots projectiles
    Caster,  // Patrols, can cast while moving, uses magic
    Melee    // Aggressive chase and melee attack
}

public enum EnemyState
{
    Idle,
    Patrolling,
    Chasing,
    Retreating,
    Attacking,
    Dead
}

#endregion

#region Enemy Projectile Component

// EnemyProjectile.cs - Component for enemy projectiles
public class EnemyProjectile : MonoBehaviour
{
    public int damage = 1;
    public bool destroyOnHit = true;
    public GameObject hitEffectPrefab;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerController player = other.GetComponent<PlayerController>();
            if (player != null && player.IsActive() && !player.IsDead())
            {
                player.TakeDamage(damage);

                // Spawn hit effect
                if (hitEffectPrefab != null)
                {
                    GameObject effect = Instantiate(hitEffectPrefab, transform.position, Quaternion.identity);
                    Destroy(effect, 1f);
                }

                if (destroyOnHit)
                {
                    Destroy(gameObject);
                }
            }
        }
        else if (other.CompareTag("Ground"))
        {
            // Spawn hit effect
            if (hitEffectPrefab != null)
            {
                GameObject effect = Instantiate(hitEffectPrefab, transform.position, Quaternion.identity);
                Destroy(effect, 1f);
            }

            Destroy(gameObject);
        }
    }
}

#endregion