using UnityEngine;

public class DragonBoss : MonoBehaviour
{
    [Header("Boss Stats")]
    public int maxHealth = 50;
    public int damage = 2;
    public float moveSpeed = 3f;

    [Header("Attack Settings")]
    public float attackRange = 120f;
    public float attackCooldown = 2f;
    public float specialAttackCooldown = 8f;
    //public GameObject fireballPrefab;
    //public Transform fireballSpawnPoint;

    private int currentHealth;
    private Animator animator;
    private Rigidbody2D rb;
    private Transform player;
    private float attackTimer = 0f;
    private float specialAttackTimer = 0f;
    private bool isDead = false;
    private bool isStunned = false;
    private float stunTimer = 0f;
    private int facingDirection = 1;

    // Animation hashes
    private int idleHash;
    private int moveHash;
    private int attackHash;
    private int specialAttackHash;
    private int jumpHash;
    private int stunHash;
    private int deathHash;
    private int talkHash;

    void Start()
    {
        currentHealth = maxHealth;
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();

        // Find player
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        //if (playerObj != null)
        //{
        //    player = playerObj.transform;
        //}

        if (player == null || !player.gameObject.activeSelf)
        {
            GameObject[] players = GameObject.FindGameObjectsWithTag("Player");

            foreach (var p in players)
            {
                PlayerController pc = p.GetComponent<PlayerController>();
                if (pc != null && pc.IsActive())
                {
                    player = playerObj.transform;
                }
            }
        }

        // Cache animation hashes
        idleHash = Animator.StringToHash("IdleDragon");
        moveHash = Animator.StringToHash("MoveTrigger");
        attackHash = Animator.StringToHash("AttackTrigger");
        specialAttackHash = Animator.StringToHash("SpecialATrigger");
        jumpHash = Animator.StringToHash("JumpTrigger");
        stunHash = Animator.StringToHash("StunedTrigger");
        deathHash = Animator.StringToHash("DeathTrigger");
        talkHash = Animator.StringToHash("TalkTrigger");

        // Intro talk animation
        StartCoroutine(IntroSequence());
    }

    System.Collections.IEnumerator IntroSequence()
    {
        PlayAnimation(talkHash);
        yield return new WaitForSeconds(5f);
    }

    void Update()
    {
        if (isDead) return;

        // Update timers
        if (attackTimer > 0) attackTimer -= Time.deltaTime;
        if (specialAttackTimer > 0) specialAttackTimer -= Time.deltaTime;

        // Handle stun
        if (isStunned)
        {
            stunTimer -= Time.deltaTime;
            if (stunTimer <= 0)
            {
                isStunned = false;
            }
            else
            {
                PlayAnimation(stunHash);
                return;
            }
        }

        // Find closest player
        if (player == null || !player.gameObject.activeSelf)
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
        if (distanceToPlayer <= attackRange)
        {
            // In attack range
            if (specialAttackTimer <= 0)
            {
                StartCoroutine(SpecialAttack());
            }
            else if (attackTimer <= 0)
            {
                StartCoroutine(Attack());
            }
            else
            {
                PlayAnimation(idleHash);
            }
        }
        else
        {
            // Move towards player
            MoveTowardsPlayer();
        }
    }

    void MoveTowardsPlayer()
    {
        if (player == null) return;

        Vector2 direction = (player.position - transform.position).normalized;
        rb.linearVelocity = new Vector2(direction.x * moveSpeed, rb.linearVelocity.y);
        PlayAnimation(moveHash);
    }

    System.Collections.IEnumerator Attack()
    {
        attackTimer = attackCooldown;
        PlayAnimation(attackHash);

        yield return new WaitForSeconds(0.5f);

        // Damage player if in range
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

    System.Collections.IEnumerator SpecialAttack()
    {
        specialAttackTimer = specialAttackCooldown;
        PlayAnimation(specialAttackHash);
        yield return new WaitForSeconds(0.7f);
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
            animator.SetTrigger(animHash);
        }
    }

    public void TakeDamage(int amount)
    {
        if (isDead) return;

        currentHealth -= amount;
        StartCoroutine(FlashRed());

        // Stun chance (20%)
        if (Random.value < 0.2f)
        {
            isStunned = true;
            stunTimer = 2f;
        }

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
        rb.linearVelocity = Vector2.zero;
        PlayAnimation(deathHash);

        GameManager.instance.AddScoreToAll(1000);
        GameManager.instance.BossDefeated();

        Destroy(gameObject, 3f);
    }

    void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerController pc = collision.gameObject.GetComponent<PlayerController>();
            if (pc != null && pc.IsActive())
            {
                pc.TakeDamage(damage);
            }
        }
    }
}
