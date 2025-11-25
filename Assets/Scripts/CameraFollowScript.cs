using UnityEngine;

// CameraFollow.cs - Attach to Main Camera
public class CameraFollow : MonoBehaviour
{
    public Transform target;
    public Transform target2;
    public float smoothSpeed = 0.125f;
    public Vector3 offset = new Vector3(0, 2, -10);
    public float minX = -100f;
    public float maxX = 100f;

    [Header("Dynamic Zoom")]
    public float minZoom = 5f;
    public float maxZoom = 15f;
    public float zoomLimiter = 50f;
    public float zoomSpeed = 3f;

    private Camera cam;

    void Start()
    {
        cam = GetComponent<Camera>();
    }

    void LateUpdate()
    {
        Vector3 targetPos;

        PlayerController p2 = target2 != null ? target2.GetComponent<PlayerController>() : null;
        bool p2Active = p2 != null && p2.IsActive();

        if (target != null && target2 != null && p2Active)
        {
            // Center between both players
            targetPos = (target.position + target2.position) / 2f;

            // Calculate distance between players
            float distance = Vector3.Distance(target.position, target2.position);

            // Calculate desired zoom based on distance
            float desiredZoom = Mathf.Lerp(minZoom, maxZoom, distance / zoomLimiter);

            // Smoothly adjust camera zoom
            if (cam != null)
            {
                cam.orthographicSize = Mathf.Lerp(cam.orthographicSize, desiredZoom, Time.deltaTime * zoomSpeed);
            }
        }
        else if (target != null)
        {
            // Single player - use target position and min zoom
            targetPos = target.position;

            // Return to min zoom when single player
            if (cam != null)
            {
                cam.orthographicSize = Mathf.Lerp(cam.orthographicSize, minZoom, Time.deltaTime * zoomSpeed);
            }
        }
        else
        {
            return;
        }

        Vector3 desiredPosition = targetPos + offset;
        desiredPosition.x = Mathf.Clamp(desiredPosition.x, minX, maxX);

        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
        smoothedPosition.z = -10;
        smoothedPosition.y += 0.4f;
        transform.position = smoothedPosition;
    }
}