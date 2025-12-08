using UnityEngine;

// SmokeEffect.cs - Creates a smoke/dust cloud effect when enemies die
// Attach to an empty GameObject, or this script creates its own particle system
public class SmokeEffect : MonoBehaviour
{
    [Header("Smoke Settings")]
    public Color smokeColor = new Color(0.7f, 0.7f, 0.7f, 0.8f); // Light gray
    public float particleLifetime = 1.5f;
    public int particleCount = 20;
    public float emissionDuration = 0.5f;
    public float particleSize = 0.5f;
    public float particleSpeed = 2f;

    private ParticleSystem smokeParticles;

    void Start()
    {
        CreateSmokeEffect();
        
        // Auto-destroy after particles are done
        Destroy(gameObject, particleLifetime + emissionDuration + 1f);
    }

    void CreateSmokeEffect()
    {
        // Get or create particle system
        smokeParticles = GetComponent<ParticleSystem>();
        
        if (smokeParticles == null)
        {
            smokeParticles = gameObject.AddComponent<ParticleSystem>();
        }

        // Main module
        var main = smokeParticles.main;
        main.startLifetime = particleLifetime;
        main.startSpeed = new ParticleSystem.MinMaxCurve(particleSpeed * 0.5f, particleSpeed);
        main.startSize = new ParticleSystem.MinMaxCurve(particleSize * 0.5f, particleSize * 1.5f);
        main.startRotation = new ParticleSystem.MinMaxCurve(0, 360f * Mathf.Deg2Rad);
        main.startColor = smokeColor;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.maxParticles = particleCount;
        main.loop = false;
        main.playOnAwake = true;
        main.gravityModifier = -0.3f; // Slight upward drift

        // Emission
        var emission = smokeParticles.emission;
        emission.enabled = true;
        emission.rateOverTime = 0;
        
        // Burst emission
        var burst = new ParticleSystem.Burst(0f, particleCount);
        emission.SetBurst(0, burst);

        // Shape - sphere for explosion effect
        var shape = smokeParticles.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 0.3f;
        shape.radiusThickness = 1f;

        // Color over lifetime - fade out
        var colorOverLifetime = smokeParticles.colorOverLifetime;
        colorOverLifetime.enabled = true;
        
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[] { 
                new GradientColorKey(smokeColor, 0f),
                new GradientColorKey(smokeColor, 1f)
            },
            new GradientAlphaKey[] { 
                new GradientAlphaKey(0.8f, 0f),
                new GradientAlphaKey(0.5f, 0.5f),
                new GradientAlphaKey(0f, 1f)
            }
        );
        colorOverLifetime.color = new ParticleSystem.MinMaxGradient(gradient);

        // Size over lifetime - grow then shrink
        var sizeOverLifetime = smokeParticles.sizeOverLifetime;
        sizeOverLifetime.enabled = true;
        
        AnimationCurve sizeCurve = new AnimationCurve();
        sizeCurve.AddKey(0f, 0.3f);
        sizeCurve.AddKey(0.3f, 1.2f);
        sizeCurve.AddKey(1f, 0.5f);
        sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, sizeCurve);

        // Velocity over lifetime - slow down
        var velocityOverLifetime = smokeParticles.velocityOverLifetime;
        velocityOverLifetime.enabled = true;
        velocityOverLifetime.space = ParticleSystemSimulationSpace.World;
        
        AnimationCurve velocityCurve = new AnimationCurve();
        velocityCurve.AddKey(0f, 1f);
        velocityCurve.AddKey(1f, 0.2f);
        velocityOverLifetime.speedModifier = new ParticleSystem.MinMaxCurve(1f, velocityCurve);

        // Rotation over lifetime
        var rotationOverLifetime = smokeParticles.rotationOverLifetime;
        rotationOverLifetime.enabled = true;
        rotationOverLifetime.z = new ParticleSystem.MinMaxCurve(-180f * Mathf.Deg2Rad, 180f * Mathf.Deg2Rad);

        // Renderer
        var renderer = smokeParticles.GetComponent<ParticleSystemRenderer>();
        renderer.renderMode = ParticleSystemRenderMode.Billboard;
        renderer.sortingLayerName = "Player";
        renderer.sortingOrder = 10;
        
        // Create material
        renderer.material = CreateSmokeMaterial();

        // Play the effect
        smokeParticles.Play();
    }

    Material CreateSmokeMaterial()
    {
        // Create a soft particle material
        Material mat = new Material(Shader.Find("Particles/Standard Unlit"));
        mat.SetColor("_Color", Color.white);
        mat.SetFloat("_Mode", 2); // Fade mode
        mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        mat.SetInt("_ZWrite", 0);
        mat.EnableKeyword("_ALPHABLEND_ON");
        mat.renderQueue = 3000;
        
        return mat;
    }

    /// <summary>
    /// Spawn a smoke effect at a position
    /// </summary>
    public static void SpawnSmoke(Vector3 position)
    {
        GameObject smokeObj = new GameObject("SmokeEffect");
        smokeObj.transform.position = position;
        smokeObj.AddComponent<SmokeEffect>();
    }
}
