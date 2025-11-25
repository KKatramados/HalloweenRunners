using UnityEngine;

// MovingPlatform.cs - Attach to platform GameObjects
// Supports horizontal, vertical, and custom path movement
public class MovingPlatform : MonoBehaviour
{
    [Header("Movement Type")]
    public MovementType movementType = MovementType.Horizontal;
    
    [Header("Movement Settings")]
    public float moveSpeed = 2f;
    public float moveDistance = 5f;
    public bool smoothMovement = true;
    
    [Header("Wait Time")]
    public float waitTimeAtEndpoints = 1f;
    
    [Header("Custom Path (for MovementType.Custom)")]
    public Transform[] waypoints; // Custom waypoints for complex paths
    
    [Header("Platform Settings")]
    public bool startMovingImmediately = true;
    public bool loopMovement = true;
    
    private Vector3 startPosition;
    private Vector3 targetPosition;
    private int currentWaypointIndex = 0;
    private float waitTimer = 0f;
    private bool isWaiting = false;
    private bool movingForward = true;
    
    private Rigidbody2D rb;
    
    public enum MovementType
    {
        Horizontal,
        Vertical,
        Custom
    }
    
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
        }
        
        // Configure Rigidbody2D for moving platform
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.gravityScale = 0f;
        
        startPosition = transform.position;
        
        // Setup based on movement type
        switch (movementType)
        {
            case MovementType.Horizontal:
                targetPosition = startPosition + Vector3.right * moveDistance;
                break;
                
            case MovementType.Vertical:
                targetPosition = startPosition + Vector3.up * moveDistance;
                break;
                
            case MovementType.Custom:
                if (waypoints != null && waypoints.Length > 0)
                {
                    targetPosition = waypoints[0].position;
                }
                else
                {
                    Debug.LogWarning("MovingPlatform: No waypoints assigned for Custom movement type!");
                    targetPosition = startPosition;
                }
                break;
        }
    }
    
    void FixedUpdate()
    {
        if (!startMovingImmediately) return;
        
        if (isWaiting)
        {
            waitTimer -= Time.fixedDeltaTime;
            if (waitTimer <= 0f)
            {
                isWaiting = false;
                UpdateTarget();
            }
            return;
        }
        
        MovePlatform();
    }
    
    void MovePlatform()
    {
        Vector3 currentPos = transform.position;
        Vector3 newPos;
        
        if (smoothMovement)
        {
            // Smooth movement using MoveTowards
            newPos = Vector3.MoveTowards(currentPos, targetPosition, moveSpeed * Time.fixedDeltaTime);
        }
        else
        {
            // Linear movement
            Vector3 direction = (targetPosition - currentPos).normalized;
            newPos = currentPos + direction * moveSpeed * Time.fixedDeltaTime;
        }
        
        // Move using Rigidbody2D for proper physics interactions
        rb.MovePosition(newPos);
        
        // Check if reached target
        if (Vector3.Distance(transform.position, targetPosition) < 0.05f)
        {
            transform.position = targetPosition;
            
            if (waitTimeAtEndpoints > 0f)
            {
                isWaiting = true;
                waitTimer = waitTimeAtEndpoints;
            }
            else
            {
                UpdateTarget();
            }
        }
    }
    
    void UpdateTarget()
    {
        switch (movementType)
        {
            case MovementType.Horizontal:
            case MovementType.Vertical:
                // Swap between start and end positions
                if (movingForward)
                {
                    movingForward = false;
                }
                else
                {
                    movingForward = true;
                }
                
                Vector3 temp = targetPosition;
                targetPosition = startPosition;
                startPosition = temp;
                break;
                
            case MovementType.Custom:
                if (waypoints == null || waypoints.Length == 0) return;
                
                // Move to next waypoint
                if (movingForward)
                {
                    currentWaypointIndex++;
                    
                    if (currentWaypointIndex >= waypoints.Length)
                    {
                        if (loopMovement)
                        {
                            currentWaypointIndex = 0;
                        }
                        else
                        {
                            currentWaypointIndex = waypoints.Length - 1;
                            movingForward = false;
                        }
                    }
                }
                else
                {
                    currentWaypointIndex--;
                    
                    if (currentWaypointIndex < 0)
                    {
                        if (loopMovement)
                        {
                            currentWaypointIndex = waypoints.Length - 1;
                        }
                        else
                        {
                            currentWaypointIndex = 0;
                            movingForward = true;
                        }
                    }
                }
                
                targetPosition = waypoints[currentWaypointIndex].position;
                break;
        }
    }
    
    // When player steps on platform, make them a child so they move with it
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            collision.transform.SetParent(transform);
        }
    }
    
    void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            collision.transform.SetParent(null);
        }
    }
    
    void OnDrawGizmos()
    {
        // Visualize platform path in editor
        Vector3 start = Application.isPlaying ? startPosition : transform.position;
        
        Gizmos.color = Color.green;
        
        switch (movementType)
        {
            case MovementType.Horizontal:
                Vector3 horizontalEnd = start + Vector3.right * moveDistance;
                Gizmos.DrawLine(start, horizontalEnd);
                Gizmos.DrawWireSphere(start, 0.3f);
                Gizmos.DrawWireSphere(horizontalEnd, 0.3f);
                break;
                
            case MovementType.Vertical:
                Vector3 verticalEnd = start + Vector3.up * moveDistance;
                Gizmos.DrawLine(start, verticalEnd);
                Gizmos.DrawWireSphere(start, 0.3f);
                Gizmos.DrawWireSphere(verticalEnd, 0.3f);
                break;
                
            case MovementType.Custom:
                if (waypoints != null && waypoints.Length > 0)
                {
                    Gizmos.color = Color.cyan;
                    
                    for (int i = 0; i < waypoints.Length; i++)
                    {
                        if (waypoints[i] != null)
                        {
                            Gizmos.DrawWireSphere(waypoints[i].position, 0.3f);
                            
                            if (i < waypoints.Length - 1 && waypoints[i + 1] != null)
                            {
                                Gizmos.DrawLine(waypoints[i].position, waypoints[i + 1].position);
                            }
                        }
                    }
                    
                    // Draw line back to start if looping
                    if (loopMovement && waypoints[0] != null && waypoints[waypoints.Length - 1] != null)
                    {
                        Gizmos.color = Color.yellow;
                        Gizmos.DrawLine(waypoints[waypoints.Length - 1].position, waypoints[0].position);
                    }
                }
                break;
        }
    }
}
