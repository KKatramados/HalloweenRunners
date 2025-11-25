using UnityEngine;

public class CoinRotate : MonoBehaviour
{
    public float rotationSpeed = 180f;
    public bool rotateOnYAxis = true;
    public float bobSpeed = 2f;
    public float bobHeight = 0.2f;

    private Vector3 startPos;

    void Start()
    {
        startPos = transform.position;
    }

    void Update()
    {
        // Rotate the coin
        if (rotateOnYAxis)
        {
            transform.Rotate(0, rotationSpeed * Time.deltaTime, 0);
        }
        else
        {
            transform.Rotate(0, 0, rotationSpeed * Time.deltaTime);
        }

        // Optional bobbing motion
        if (bobHeight > 0)
        {
            float newY = startPos.y + Mathf.Sin(Time.time * bobSpeed) * bobHeight;
            transform.position = new Vector3(transform.position.x, newY, transform.position.z);
        }
    }
}
