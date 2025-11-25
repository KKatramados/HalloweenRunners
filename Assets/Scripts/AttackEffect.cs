using UnityEngine;

public class AttackEffect : MonoBehaviour
{
    public float rotationSpeed = 360f;
    public float scaleSpeed = 2f;
    private float startScale = 0.5f;
    private float endScale = 1.5f;
    private float timer = 0f;
    
    void Start()
    {
        transform.localScale = Vector3.one * startScale;
    }
    
    void Update()
    {
        // Rotate
        transform.Rotate(0, 0, rotationSpeed * Time.deltaTime);
        
        // Scale up
        timer += Time.deltaTime * scaleSpeed;
        float scale = Mathf.Lerp(startScale, endScale, timer);
        transform.localScale = Vector3.one * scale;
        
        // Fade out
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            Color c = sr.color;
            c.a = 1f - timer;
            sr.color = c;
        }
    }
}

