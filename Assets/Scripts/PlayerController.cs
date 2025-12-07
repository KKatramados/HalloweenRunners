using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

// PlayerController.cs - Updated for Unity New Input System
// Attach to player GameObject (Single player on mobile, multiplayer on desktop)
public class PlayerController : MonoBehaviour
{
    [Header("Player Settings")]
    public int playerNumber = 1; // 1 or 2

    [Header("Movement")]
    public float moveSpeed = 5f;
    public float jumpForce = 12f;
    public int maxHealth = 6; // 6 half-hearts = 3 full hearts

    [Header("Jump Settings")]
    public int maxJumps = 2;
    public float jumpCooldown = 1f;

    [Header("Ground Check")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;
    public LayerMask groundLayer;

    [Header("Attack")]
    public GameObject attackEffectPrefab;
    public Transform attackPoint;

    [Header("Input Actions")]
    public InputActionAsset inputActions;

    [Header("Mobile Touch Input")]
    public bool isMobileDevice = false;
    public float touchDeadZone = 0.1f;

    [Header("Respawn Settings")]
    public float respawnDelay = 3f;
    public Vector3 respawnOffset = new Vector3(2f, 0f, 0f);

    private bool isWaitingToRespawn = false;
    private float respawnTimer = 0f;

    private Rigidbody2D rb;
    private Animator animator;
    private bool isGrounded;
    private bool facingRight = true;
    private int currentHealth;
    private float attackCooldown = 0f;
    private bool isAttacking = false;
    private int jumpsRemaining;
    private float jumpCooldownTimer = 0f;
    private bool isHurt = false;
    private bool isDead = false;
    private bool isActive = false;

    // New Input System variables
    private InputActionMap playerActionMap;
    private InputAction moveAction;
    private InputAction jumpAction;
    private InputAction attackAction;
    private InputAction joinAction;
    private InputAction respawnAction;

    // Touch input variables (for mobile)
    private int leftTouchId = -1;
    private int rightTouchId = -1;
    private Vector2 touchStartPos;
    private float horizontalInput = 0f;
    private bool jumpPressed = false;
    private bool attackPressed = false;

    // Touch zones (screen split into regions)
    private Rect leftMovementZone;
    private Rect rightButtonZone;
    private Rect jumpButtonZone;
    private Rect attackButtonZone;

    // Animation parameter hashes
    private int idleHash;
    private int runHash;
    private int jumpHash;
    private int attackHash;
    private int hurtHash;
    private int crouchHash;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();

        // Detect mobile device
        isMobileDevice = Application.isMobilePlatform;

        // Setup Input Actions
        SetupInputActions();
    }

    void SetupInputActions()
    {
        if (inputActions == null)
        {
            Debug.LogError("Input Actions asset not assigned to PlayerController!");
            return;
        }

        // Get the correct action map based on player number and platform
        if (isMobileDevice && playerNumber == 1)
        {
            // Mobile uses touch-based input processed manually
            playerActionMap = inputActions.FindActionMap("Mobile");
        }
        else if (playerNumber == 1)
        {
            playerActionMap = inputActions.FindActionMap("Player1");
        }
        else if (playerNumber == 2)
        {
            playerActionMap = inputActions.FindActionMap("Player2");
        }

        if (playerActionMap == null)
        {
            Debug.LogError($"Could not find action map for Player {playerNumber}");
            return;
        }

        // Get individual actions (desktop/gamepad)
        if (!isMobileDevice || playerNumber == 2)
        {
            moveAction = playerActionMap.FindAction("Move");
            jumpAction = playerActionMap.FindAction("Jump");
            attackAction = playerActionMap.FindAction("Attack");
            respawnAction = playerActionMap.FindAction("Respawn");

            if (playerNumber == 2)
            {
                joinAction = playerActionMap.FindAction("Join");
            }
        }
    }

    void Start()
    {
        currentHealth = maxHealth;
        jumpsRemaining = maxJumps;

        // Setup touch zones if mobile
        if (isMobileDevice)
        {
            SetupTouchZones();
        }

        // Cache animation hashes
        idleHash = Animator.StringToHash("Idle");
        runHash = Animator.StringToHash("Run");
        jumpHash = Animator.StringToHash("Jump");
        attackHash = Animator.StringToHash("Attack");
        hurtHash = Animator.StringToHash("Hurt");
        crouchHash = Animator.StringToHash("Crouch");

        // MOBILE: Only Player 1 is active, Player 2 is disabled
        if (isMobileDevice)
        {
            if (playerNumber == 1)
            {
                Activate();
                GameManager.instance.UpdatePlayerHealth(playerNumber, currentHealth);
            }
            else
            {
                // Disable Player 2 completely on mobile
                gameObject.SetActive(false);
                Debug.Log("Mobile detected: Player 2 disabled (single player only)");
            }
        }
        else
        {
            // Desktop: Normal behavior (Player 1 active, Player 2 can join)
            if (playerNumber == 1)
            {
                Activate();
                GameManager.instance.UpdatePlayerHealth(playerNumber, currentHealth);
            }
            else
            {
                Deactivate();
            }
        }
    }

    void OnEnable()
    {
        if (playerActionMap != null)
        {
            playerActionMap.Enable();
        }
    }

    void OnDisable()
    {
        if (playerActionMap != null)
        {
            playerActionMap.Disable();
        }
    }

    void SetupTouchZones()
    {
        float screenWidth = Screen.width;
        float screenHeight = Screen.height;

        leftMovementZone = new Rect(0, 0, screenWidth * 0.4f, screenHeight);
        rightButtonZone = new Rect(screenWidth * 0.4f, 0, screenWidth * 0.6f, screenHeight);
        jumpButtonZone = new Rect(screenWidth * 0.75f, screenHeight * 0.1f, screenWidth * 0.2f, screenHeight * 0.2f);
        attackButtonZone = new Rect(screenWidth * 0.75f, screenHeight * 0.35f, screenWidth * 0.2f, screenHeight * 0.2f);
    }

    void Update()
    {
        // Handle respawn countdown
        if (isWaitingToRespawn)
        {
            respawnTimer -= Time.deltaTime;

            if (respawnTimer <= 0f)
            {
                // Check for rejoin input
                if (GetRejoinInput())
                {
                    Respawn();
                }
            }

            return;
        }

        // Process touch input if mobile
        if (isMobileDevice)
        {
            ProcessTouchInput();
        }

        // MOBILE: Disable Player 2 join completely
        if (!isActive && playerNumber == 2)
        {
            if (isMobileDevice)
            {
                return;
            }
            else
            {
                // Desktop: Allow Player 2 to join
                if (joinAction != null && joinAction.WasPressedThisFrame())
                {
                    Activate();
                    GameManager.instance.Player2Joined();
                    return;
                }
                else
                {
                    return;
                }
            }
        }

        if (isDead) return;

        // Ground check
        bool wasGrounded = isGrounded;
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

        // Reset jumps when landing
        if (isGrounded && !wasGrounded)
        {
            jumpsRemaining = maxJumps;
            jumpCooldownTimer = 0f;
        }

        // Jump cooldown
        if (jumpCooldownTimer > 0)
        {
            jumpCooldownTimer -= Time.deltaTime;
            if (jumpCooldownTimer <= 0)
            {
                jumpsRemaining = maxJumps;
            }
        }

        // Don't move during attack or hurt animation
        if (!isAttacking && !isHurt)
        {
            // Movement input
            float moveInput = GetHorizontalInput();
            rb.linearVelocity = new Vector2(moveInput * moveSpeed, rb.linearVelocity.y);

            // Flip sprite
            if (moveInput > 0 && !facingRight)
                Flip();
            else if (moveInput < 0 && facingRight)
                Flip();

            // Jump
            if (GetJumpInput() && jumpsRemaining > 0 && jumpCooldownTimer <= 0)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
                jumpsRemaining--;

                // AUDIO: Play jump sound
                if (AudioManager.instance != null)
                    AudioManager.instance.PlayJump();

                if (jumpsRemaining <= 0)
                {
                    jumpCooldownTimer = jumpCooldown;
                }
            }

            // Update animations
            UpdateAnimations(moveInput);
        }

        // Attack
        if (attackCooldown > 0)
            attackCooldown -= Time.deltaTime;

        if (GetAttackInput() && attackCooldown <= 0 && !isAttacking && !isHurt)
        {
            StartCoroutine(AttackCoroutine());
        }

        // Reset touch input flags at end of frame
        if (isMobileDevice)
        {
            jumpPressed = false;
            attackPressed = false;
        }
    }

    bool GetRejoinInput()
    {
        if (isMobileDevice)
        {
            return jumpPressed || attackPressed;
        }
        else
        {
            return respawnAction != null && respawnAction.WasPressedThisFrame();
        }
    }

    void ProcessTouchInput()
    {
        // Reset input
        horizontalInput = 0f;

        // Process all active touches
        for (int i = 0; i < Touchscreen.current.touches.Count; i++)
        {
            var touch = Touchscreen.current.touches[i];
            
            if (!touch.isInProgress)
                continue;

            Vector2 touchPos = touch.position.ReadValue();

            // Check which zone the touch is in
            if (leftMovementZone.Contains(touchPos))
            {
                ProcessMovementTouch(touch);
            }
            else if (jumpButtonZone.Contains(touchPos))
            {
                if (touch.phase.ReadValue() == UnityEngine.InputSystem.TouchPhase.Began)
                {
                    jumpPressed = true;
                }
            }
            else if (attackButtonZone.Contains(touchPos))
            {
                if (touch.phase.ReadValue() == UnityEngine.InputSystem.TouchPhase.Began)
                {
                    attackPressed = true;
                }
            }
        }

        // Alternative: Swipe gestures (if not using specific button zones)
        if (Touchscreen.current != null && Touchscreen.current.touches.Count > 0)
        {
            var touch = Touchscreen.current.touches[0];
            Vector2 touchPos = touch.position.ReadValue();

            bool inJumpZone = jumpButtonZone.Contains(touchPos);
            bool inAttackZone = attackButtonZone.Contains(touchPos);

            if (rightButtonZone.Contains(touchPos) && !inJumpZone && !inAttackZone)
            {
                if (touch.phase.ReadValue() == UnityEngine.InputSystem.TouchPhase.Began)
                {
                    touchStartPos = touchPos;
                }
                else if (touch.phase.ReadValue() == UnityEngine.InputSystem.TouchPhase.Moved ||
                         touch.phase.ReadValue() == UnityEngine.InputSystem.TouchPhase.Ended)
                {
                    Vector2 swipeDelta = touchPos - touchStartPos;

                    // Swipe up = jump
                    if (swipeDelta.y > Screen.height * 0.1f && Mathf.Abs(swipeDelta.x) < Screen.width * 0.1f)
                    {
                        if (touch.phase.ReadValue() == UnityEngine.InputSystem.TouchPhase.Ended)
                        {
                            jumpPressed = true;
                        }
                    }

                    // Tap = attack
                    if (touch.phase.ReadValue() == UnityEngine.InputSystem.TouchPhase.Ended && 
                        swipeDelta.magnitude < Screen.width * 0.05f)
                    {
                        attackPressed = true;
                    }
                }
            }
        }
    }

    void ProcessMovementTouch(TouchControl touch)
    {
        Vector2 touchPos = touch.position.ReadValue();
        var phase = touch.phase.ReadValue();

        if (phase == UnityEngine.InputSystem.TouchPhase.Began)
        {
            leftTouchId = touch.touchId.ReadValue();
            touchStartPos = touchPos;
        }
        else if (touch.touchId.ReadValue() == leftTouchId)
        {
            if (phase == UnityEngine.InputSystem.TouchPhase.Moved || 
                phase == UnityEngine.InputSystem.TouchPhase.Stationary)
            {
                Vector2 delta = touchPos - touchStartPos;
                float normalizedX = delta.x / (Screen.width * 0.2f);
                horizontalInput = Mathf.Clamp(normalizedX, -1f, 1f);

                if (Mathf.Abs(horizontalInput) < touchDeadZone)
                {
                    horizontalInput = 0f;
                }
            }
            else if (phase == UnityEngine.InputSystem.TouchPhase.Ended || 
                     phase == UnityEngine.InputSystem.TouchPhase.Canceled)
            {
                leftTouchId = -1;
                horizontalInput = 0f;
            }
        }
    }

    float GetHorizontalInput()
    {
        if (isMobileDevice)
        {
            return horizontalInput;
        }
        else
        {
            if (moveAction != null)
            {
                Vector2 moveVector = moveAction.ReadValue<Vector2>();
                return moveVector.x;
            }
            return 0f;
        }
    }

    bool GetJumpInput()
    {
        if (isMobileDevice)
        {
            return jumpPressed;
        }
        else
        {
            return jumpAction != null && jumpAction.WasPressedThisFrame();
        }
    }

    bool GetAttackInput()
    {
        if (isMobileDevice)
        {
            return attackPressed;
        }
        else
        {
            return attackAction != null && attackAction.WasPressedThisFrame();
        }
    }

    void Activate()
    {
        isActive = true;
        gameObject.SetActive(true);

        if (playerNumber == 2)
        {
            GameObject player1 = GameObject.FindGameObjectWithTag("Player");
            if (player1 != null && player1 != gameObject)
            {
                transform.position = player1.transform.position + Vector3.right * 1.5f;
            }

            GameManager.instance.UpdatePlayerHealth(playerNumber, currentHealth);
        }

        if (rb != null) rb.simulated = true;

        SpriteRenderer[] sprites = GetComponentsInChildren<SpriteRenderer>();
        foreach (var sr in sprites)
        {
            sr.enabled = true;
        }

        // Enable input
        if (playerActionMap != null)
        {
            playerActionMap.Enable();
        }
    }

    void Deactivate()
    {
        isActive = false;

        if (rb != null) rb.simulated = false;

        SpriteRenderer[] sprites = GetComponentsInChildren<SpriteRenderer>();
        foreach (var sr in sprites)
        {
            sr.enabled = false;
        }

        // Keep input enabled for Player 2 to detect join button
        if (playerNumber != 2 && playerActionMap != null)
        {
            playerActionMap.Disable();
        }
    }

    void UpdateAnimations(float moveInput)
    {
        if (!isGrounded)
        {
            PlayAnimation(jumpHash);
        }
        else if (Mathf.Abs(moveInput) > 0.1f)
        {
            PlayAnimation(runHash);
        }
        else
        {
            PlayAnimation(idleHash);
        }
    }

    void PlayAnimation(int animHash)
    {
        if (animator != null)
        {
            animator.Play(animHash);
        }
    }

    System.Collections.IEnumerator AttackCoroutine()
    {
        isAttacking = true;
        attackCooldown = 0.5f;

        if (animator != null)
        {
            animator.Play(attackHash);
        }

        yield return new WaitForSeconds(0.1f);
        Attack();

        yield return new WaitForSeconds(0.3f);
        isAttacking = false;
    }

    void Flip()
    {
        facingRight = !facingRight;
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }

    void Attack()
    {
        if (attackEffectPrefab != null)
        {
            Vector3 spawnPos = attackPoint != null ? attackPoint.position : transform.position;
            GameObject projectile = Instantiate(attackEffectPrefab, spawnPos, Quaternion.identity);

            Projectile projScript = projectile.GetComponent<Projectile>();
            if (projScript != null)
            {
                projScript.Initialize(facingRight ? 1 : -1);
            }
        }

        if (AudioManager.instance != null)
            AudioManager.instance.PlayAttack();
    }

    public void TakeDamage(int damage)
    {
        if (isHurt || isDead || !isActive) return;

        currentHealth -= damage;
        if (currentHealth < 0) currentHealth = 0;

        // Show damage text
        if (DamageTextManager.instance != null)
        {
            DamageTextManager.instance.ShowDamageWithCritical(damage, transform.position, false);
        }

        if (AudioManager.instance != null)
            AudioManager.instance.PlayHurt();

        StartCoroutine(HurtCoroutine());
        GameManager.instance.UpdatePlayerHealth(playerNumber, currentHealth);

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    System.Collections.IEnumerator HurtCoroutine()
    {
        isHurt = true;

        if (animator != null)
        {
            animator.Play(hurtHash);
        }

        SpriteRenderer[] spriteRenderers = GetComponentsInChildren<SpriteRenderer>();
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

        yield return new WaitForSeconds(0.3f);
        isHurt = false;
    }

    void Die()
    {
        if (isDead) return;

        isDead = true;
        isWaitingToRespawn = false;
        rb.linearVelocity = Vector2.zero;
        enabled = false;

        if (AudioManager.instance != null)
            AudioManager.instance.PlayDeath();

        if (animator != null)
        {
            animator.Play(crouchHash);
        }

        if (GameManager.instance != null)
        {
            GameManager.instance.PlayerDied(playerNumber);
        }

        StartCoroutine(DeathSequence());
    }

    System.Collections.IEnumerator DeathSequence()
    {
        yield return new WaitForSeconds(2f);

        if (GameManager.instance != null && !GameManager.instance.IsGameOver())
        {
            StartCoroutine(MoveOffScreen());
            yield return new WaitForSeconds(1f);

            isWaitingToRespawn = true;
            respawnTimer = respawnDelay;

            if (GameManager.instance != null)
            {
                GameManager.instance.ShowRespawnPrompt(playerNumber, true);
            }
        }
    }

    System.Collections.IEnumerator MoveOffScreen()
    {
        float duration = 1f;
        float elapsed = 0f;
        Vector3 startPos = transform.position;
        Vector3 endPos = startPos + Vector3.down * 5f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            transform.position = Vector3.Lerp(startPos, endPos, elapsed / duration);

            SpriteRenderer[] sprites = GetComponentsInChildren<SpriteRenderer>();
            foreach (var sr in sprites)
            {
                Color c = sr.color;
                c.a = Mathf.Lerp(1f, 0f, elapsed / duration);
                sr.color = c;
            }

            yield return null;
        }

        SpriteRenderer[] finalSprites = GetComponentsInChildren<SpriteRenderer>();
        foreach (var sr in finalSprites)
        {
            sr.enabled = false;
        }
    }

    public void Respawn()
    {
        Vector3 respawnPosition = GetRespawnPosition();

        isDead = false;
        isWaitingToRespawn = false;
        isHurt = false;
        isAttacking = false;
        currentHealth = maxHealth;
        jumpsRemaining = maxJumps;
        jumpCooldownTimer = 0f;

        transform.position = respawnPosition;
        rb.linearVelocity = Vector2.zero;

        SpriteRenderer[] sprites = GetComponentsInChildren<SpriteRenderer>();
        foreach (var sr in sprites)
        {
            sr.enabled = true;
            Color c = sr.color;
            c.a = 1f;
            sr.color = c;
        }

        enabled = true;

        if (GameManager.instance != null)
        {
            GameManager.instance.UpdatePlayerHealth(playerNumber, currentHealth);
            GameManager.instance.ShowRespawnPrompt(playerNumber, false);
            GameManager.instance.PlayerRespawned(playerNumber);
        }

        if (animator != null)
        {
            animator.Play(idleHash);
        }

        StartCoroutine(SpawnEffect());
        Debug.Log($"Player {playerNumber} respawned!");
    }

    System.Collections.IEnumerator SpawnEffect()
    {
        for (int i = 0; i < 3; i++)
        {
            SpriteRenderer[] sprites = GetComponentsInChildren<SpriteRenderer>();

            foreach (var sr in sprites)
            {
                sr.color = Color.white;
            }
            yield return new WaitForSeconds(0.1f);

            foreach (var sr in sprites)
            {
                sr.color = Color.white;
            }
            yield return new WaitForSeconds(0.1f);
        }
    }

    Vector3 GetRespawnPosition()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");

        foreach (var p in players)
        {
            PlayerController pc = p.GetComponent<PlayerController>();
            if (pc != null && pc != this && pc.IsActive() && !pc.isDead)
            {
                return pc.transform.position + respawnOffset;
            }
        }

        return new Vector3(0, 2, 0);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (isDead || !isActive) return;

        if (other.CompareTag("Coin"))
        {
            GameManager.instance.AddScore(10, playerNumber);

            if (AudioManager.instance != null)
                AudioManager.instance.PlayCoin();

            Destroy(other.gameObject);
        }
        else if (other.CompareTag("Health"))
        {
            if (currentHealth < maxHealth)
            {
                currentHealth += 2;
                if (currentHealth > maxHealth) currentHealth = maxHealth;
                GameManager.instance.UpdatePlayerHealth(playerNumber, currentHealth);

                if (AudioManager.instance != null)
                    AudioManager.instance.PlayHeal();

                Destroy(other.gameObject);
            }
        }
        else if (other.CompareTag("Candy"))
        {
            CandyCollectible candy = other.GetComponent<CandyCollectible>();
            if (candy != null)
            {
                GameManager.instance.AddCandy(playerNumber, candy.candyType);
                GameManager.instance.AddScore(20, playerNumber);

                if (AudioManager.instance != null)
                    AudioManager.instance.PlayCandy();

                Destroy(other.gameObject);
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }

        Vector2 attackPos = attackPoint != null ? attackPoint.position : transform.position;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(attackPos, 0.6f);
    }

    public void PlayFootstep()
    {
        if (AudioManager.instance != null && isGrounded && !isDead)
        {
            AudioManager.instance.PlayFootstep();
        }
    }

    public int GetHealth() { return currentHealth; }
    public bool IsActive() { return isActive; }
    public bool IsDead() { return isDead; }
}
