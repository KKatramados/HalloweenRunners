using UnityEngine;

// HealthPickup.cs - Attach to heart/health pickup GameObjects
public class HealthPickup : MonoBehaviour
{
    [Header("Health Settings")]
    public int healthAmount = 2; // Restores 1 full heart (2 half-hearts)

    [Header("Visual Effects")]
    public float bobSpeed = 2f;
    public float bobHeight = 0.3f;
    public float rotationSpeed = 90f;

    private Vector3 startPos;

    void Start()
    {
        startPos = transform.position;
    }

    void Update()
    {
        // Bobbing animation
        if (bobHeight > 0)
        {
            float newY = startPos.y + Mathf.Sin(Time.time * bobSpeed) * bobHeight;
            transform.position = new Vector3(transform.position.x, newY, transform.position.z);
        }

        // Rotation animation
        if (rotationSpeed > 0)
        {
            transform.Rotate(0, rotationSpeed * Time.deltaTime, 0);
        }
    }

    //void OnTriggerEnter2D(Collider2D other)
    //{
    //    if (other.CompareTag("Player"))
    //    {
    //        PlayerController player = other.GetComponent<PlayerController>();
    //        if (player != null && player.IsActive())
    //        {
    //            int currentHealth = player.GetHealth();
    //            int maxHealth = player.maxHealth;

    //            // Only heal if not at full health
    //            if (currentHealth < maxHealth)
    //            {
    //                // Calculate how much health to restore
    //                int newHealth = Mathf.Min(currentHealth + healthAmount, maxHealth);
    //                int healedAmount = newHealth - currentHealth;

    //                // Apply healing (done in PlayerController's OnTriggerEnter2D)
    //                // This pickup will be destroyed by the PlayerController

    //                // Optional: Play sound effect here
    //                // AudioSource.PlayClipAtPoint(healSound, transform.position);

    //                Destroy(gameObject);
    //            }
    //        }
    //    }
    //}
}