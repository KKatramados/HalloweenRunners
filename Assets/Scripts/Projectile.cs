using UnityEngine;

public class Projectile : MonoBehaviour
{
    public float speed = 10f;
    public float lifetime = 2.2f;
    public int damage = 1;
    public float rotationSpeed = 720f;

    private Rigidbody2D rb;
    private int direction = 1;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public void Initialize(int dir)
    {
        direction = dir;

        // AUDIO: Play projectile sound
        if (AudioManager.instance != null)
            AudioManager.instance.PlayProjectile();

        if (rb != null)
        {
            rb.linearVelocity = new Vector2(direction * speed, 0);
        }
        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        transform.Rotate(0, 0, rotationSpeed * Time.deltaTime);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy") )
        {
            var enemy = other.GetComponent<Enemy>();
            if (enemy != null)
            {
                // Check for critical hit
                bool isCritical = DamageTextManager.instance != null && DamageTextManager.instance.RollForCritical();
                int actualDamage = isCritical ? Mathf.RoundToInt(damage * 1.5f) : damage;

                enemy.TakeDamage(actualDamage);

                // Show damage text
                if (DamageTextManager.instance != null)
                {
                    DamageTextManager.instance.ShowDamageWithCritical(damage, other.transform.position, isCritical);
                }

                Destroy(gameObject);
                return;
            }

            SkeletonBoss skeletonBoss = other.GetComponent<SkeletonBoss>();
            if (skeletonBoss != null)
            {
                // Check for critical hit
                bool isCritical = DamageTextManager.instance != null && DamageTextManager.instance.RollForCritical();
                int actualDamage = isCritical ? Mathf.RoundToInt(damage * 1.5f) : damage;

                skeletonBoss.TakeDamage(actualDamage);

                // Show damage text
                if (DamageTextManager.instance != null)
                {
                    DamageTextManager.instance.ShowDamageWithCritical(damage, other.transform.position, isCritical);
                }

                Destroy(gameObject);
                return;
            }
        }
        else if (other.CompareTag("Ground"))
        {
            Destroy(gameObject);
        }
    }
}
