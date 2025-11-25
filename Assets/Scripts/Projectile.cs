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
        if (other.CompareTag("Enemy"))
        {
            Enemy enemy = other.GetComponent<Enemy>();
            if (enemy != null)
            {
                enemy.TakeDamage(damage);
                Destroy(gameObject);
            }
            SkeletonBoss boss = other.GetComponent<SkeletonBoss>();
            if (boss != null)
            {
                boss.TakeDamage(damage);
                Destroy(gameObject);
            }
        }
        else if (other.CompareTag("Ground"))
        {
            Destroy(gameObject);
        }
    }
}
