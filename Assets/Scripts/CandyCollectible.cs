using UnityEngine;

// CandyCollectible.cs - Attach to candy prefabs to identify type
public class CandyCollectible : MonoBehaviour
{
    public enum CandyType { Red, Blue, Green }
    public CandyType candyType = CandyType.Red;

    void Start()
    {
        Destroy(gameObject,3f);
    }
}