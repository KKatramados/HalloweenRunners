using UnityEngine;

// FogEffect.cs - Creates atmospheric fog effect using particle system (Unity 6)
public class FogEffect : MonoBehaviour
{
    [Header("Fog Settings")]
    public Color fogColor = new Color(0.8f, 0.8f, 0.9f, 0.5f); // Light gray-blue
    public float fogDensity = 10f; // Particles per second
    public float fogSpeed = 0.5f; // How fast fog moves
    public float fogHeight = 5f; // Height of fog area
    public float fogWidth = 20f; // Width of fog area

    [Header("Fog Behavior")]
    public Vector2 windDirection = new Vector2(1f, 0.2f); // Fog drift direction
    public bool randomizeWind = true;
    public float windStrength = 0.5f;

    [Header("Fog Sprite")]
    public Sprite fogSprite; // Assign a soft cloud/fog sprite

    private ParticleSystem fogParticles;

    void Start()
    {
        CreateFogEffect();
    }

    void CreateFogEffect()
    {
        // Create particle system GameObject
        GameObject fogObject = new GameObject("FogParticles");
        fogObject.transform.SetParent(transform);
        fogObject.transform.localPosition = Vector3.zero;

        fogParticles = fogObject.AddComponent<ParticleSystem>();

        // Main module
        var main = fogParticles.main;
        main.startLifetime = new ParticleSystem.MinMaxCurve(10f, 15f);
        main.startSpeed = new ParticleSystem.MinMaxCurve(fogSpeed * 0.5f, fogSpeed);
        main.startSize = new ParticleSystem.MinMaxCurve(2f, 5f);
        main.startRotation = new ParticleSystem.MinMaxCurve(0, 360f * Mathf.Deg2Rad);
        main.startColor = fogColor;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.maxParticles = 100;
        main.loop = true;
        main.playOnAwake = true;

        // Emission
        var emission = fogParticles.emission;
        emission.rateOverTime = fogDensity;

        // Shape (box area)
        var shape = fogParticles.shape;
        shape.shapeType = ParticleSystemShapeType.Box;
        shape.scale = new Vector3(fogWidth, fogHeight, 1f);

        // Velocity over lifetime (wind effect)
        var velocity = fogParticles.velocityOverLifetime;
        velocity.enabled = true;
        velocity.space = ParticleSystemSimulationSpace.World;
        velocity.x = new ParticleSystem.MinMaxCurve(windDirection.x * windStrength);
        velocity.y = new ParticleSystem.MinMaxCurve(windDirection.y * windStrength);

        // Size over lifetime (fade in/out)
        var sizeOverLifetime = fogParticles.sizeOverLifetime;
        sizeOverLifetime.enabled = true;
        AnimationCurve sizeCurve = new AnimationCurve();
        sizeCurve.AddKey(0f, 0f);    // Start small
        sizeCurve.AddKey(0.2f, 1f);  // Grow
        sizeCurve.AddKey(0.8f, 1f);  // Stay
        sizeCurve.AddKey(1f, 0f);    // Shrink
        sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, sizeCurve);

        // Color over lifetime (fade alpha)
        var colorOverLifetime = fogParticles.colorOverLifetime;
        colorOverLifetime.enabled = true;
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[] {
                new GradientColorKey(fogColor, 0f),
                new GradientColorKey(fogColor, 1f)
            },
            new GradientAlphaKey[] {
                new GradientAlphaKey(0f, 0f),
                new GradientAlphaKey(0.5f, 0.3f),
                new GradientAlphaKey(0.5f, 0.7f),
                new GradientAlphaKey(0f, 1f)
            }
        );
        colorOverLifetime.color = new ParticleSystem.MinMaxGradient(gradient);

        // Rotation over lifetime
        var rotationOverLifetime = fogParticles.rotationOverLifetime;
        rotationOverLifetime.enabled = true;
        rotationOverLifetime.z = new ParticleSystem.MinMaxCurve(-30f * Mathf.Deg2Rad, 30f * Mathf.Deg2Rad);

        // Renderer settings (Unity 6 compatible)
        var renderer = fogParticles.GetComponent<ParticleSystemRenderer>();
        renderer.renderMode = ParticleSystemRenderMode.Billboard;
        renderer.sortingLayerName = "Background";
        renderer.sortingOrder = 3; // Behind most objects

        // Use sprite if provided, otherwise use default material
        if (fogSprite != null)
        {
            renderer.material = CreateSpriteMaterial();
        }
        else
        {
            renderer.material = CreateDefaultFogMaterial();
        }
    }

    Material CreateSpriteMaterial()
    {
        // Create material using the assigned sprite
        Material mat = new Material(Shader.Find("Sprites/Default"));
        mat.mainTexture = fogSprite.texture;
        mat.color = fogColor;

        // Enable transparency
        mat.SetFloat("_Mode", 3); // Transparent mode
        mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        mat.SetInt("_ZWrite", 0);
        mat.DisableKeyword("_ALPHATEST_ON");
        mat.EnableKeyword("_ALPHABLEND_ON");
        mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        mat.renderQueue = 3000;

        return mat;
    }

    Material CreateDefaultFogMaterial()
    {
        // Create a default soft particle material
        Material mat = new Material(Shader.Find("Particles/Standard Unlit"));
        mat.SetColor("_Color", fogColor);
        mat.SetFloat("_Mode", 2); // Fade mode
        mat.EnableKeyword("_ALPHABLEND_ON");
        mat.renderQueue = 3000;
        return mat;
    }

    void Update()
    {
        // Optional: Randomize wind direction slightly
        if (randomizeWind && fogParticles != null)
        {
            var velocity = fogParticles.velocityOverLifetime;
            float randomX = windDirection.x + Mathf.Sin(Time.time * 0.5f) * 0.2f;
            float randomY = windDirection.y + Mathf.Cos(Time.time * 0.3f) * 0.1f;
            velocity.x = new ParticleSystem.MinMaxCurve(randomX * windStrength);
            velocity.y = new ParticleSystem.MinMaxCurve(randomY * windStrength);
        }
    }
}