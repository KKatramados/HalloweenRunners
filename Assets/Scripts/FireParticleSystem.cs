using UnityEngine;

public class Torch : MonoBehaviour
{
    [Header("Fire Settings")]
    [SerializeField] private bool startLit = true;
    [SerializeField] private float fireIntensity = 1f;
    [Range(0f, 1f)]
    [SerializeField] private float flickerAmount = 0.3f;
    [SerializeField] private float flickerSpeed = 5f;
    
    [Header("Light Settings")]
    [SerializeField] private Color lightColor = new Color(1f, 0.6f, 0.2f);
    [SerializeField] private float lightIntensity = 3f;
    [SerializeField] private float lightRange = 8f;
    [SerializeField] private bool castShadows = true;
    
    [Header("Sound Settings")]
    [SerializeField] private bool enableSound = true;
    [SerializeField] private float soundVolume = 0.3f;
    
    [Header("Particle Settings")]
    [SerializeField] private int maxParticles = 50;
    [SerializeField] private float emissionRate = 40f;
    [SerializeField] private float particleLifetime = 0.8f;
    [SerializeField] private float particleSize = 0.15f;
    [SerializeField] private float emissionRadius = 0.08f;
    
    [Header("Spark Settings")]
    [SerializeField] private bool enableSparks = true;
    [SerializeField] private float sparkEmissionRate = 15f;
    [SerializeField] private float sparkSize = 0.03f;
    
    // Components
    private ParticleSystem fireParticles;
    private ParticleSystem sparkParticles;
    private Light torchLight;
    private AudioSource audioSource;
    
    // Runtime variables
    private bool isLit = false;
    private float baseIntensity;
    private float baseLightIntensity;
    private float flickerTimer;
    private float targetFlicker = 1f;
    private float currentFlicker = 1f;
    
    void Start()
    {
        SetupTorch();
        
        if (startLit)
        {
            LightTorch();
        }
    }
    
    void SetupTorch()
    {
        // Store base values
        baseIntensity = fireIntensity;
        baseLightIntensity = lightIntensity;
        
        // Create fire particles
        CreateFireParticles();
        
        // Create spark particles
        if (enableSparks)
        {
            CreateSparkParticles();
        }
        
        // Create light
        CreateTorchLight();
        
        // Create audio source
        if (enableSound)
        {
            CreateAudioSource();
        }
    }
    
    void CreateFireParticles()
    {
        GameObject fireObj = new GameObject("TorchFire");
        fireObj.transform.SetParent(transform);
        fireObj.transform.localPosition = Vector3.zero;
        fireObj.transform.localRotation = Quaternion.identity;
        
        fireParticles = fireObj.AddComponent<ParticleSystem>();
        
        var main = fireParticles.main;
        var emission = fireParticles.emission;
        var shape = fireParticles.shape;
        var colorOverLifetime = fireParticles.colorOverLifetime;
        var sizeOverLifetime = fireParticles.sizeOverLifetime;
        var velocityOverLifetime = fireParticles.velocityOverLifetime;
        var renderer = fireParticles.GetComponent<ParticleSystemRenderer>();
        
        // Main module
        main.startLifetime = particleLifetime;
        main.startSpeed = new ParticleSystem.MinMaxCurve(1f, 2f);
        main.startSize = new ParticleSystem.MinMaxCurve(particleSize * 0.8f, particleSize * 1.2f);
        main.startRotation = new ParticleSystem.MinMaxCurve(0, 360 * Mathf.Deg2Rad);
        main.maxParticles = maxParticles;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.gravityModifier = -0.5f;
        
        // Emission
        emission.rateOverTime = emissionRate;
        
        // Shape - cone for torch flame
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Circle;
        shape.radius = emissionRadius;
        shape.radiusThickness = 1f;
        
        // Color over lifetime - torch fire colors
        colorOverLifetime.enabled = true;
        Gradient gradient = new Gradient();
        
        GradientColorKey[] colorKeys = new GradientColorKey[4];
        colorKeys[0] = new GradientColorKey(new Color(1f, 1f, 0.8f), 0f);      // Bright yellow
        colorKeys[1] = new GradientColorKey(new Color(1f, 0.7f, 0.2f), 0.3f);  // Orange-yellow
        colorKeys[2] = new GradientColorKey(new Color(1f, 0.4f, 0f), 0.6f);    // Orange
        colorKeys[3] = new GradientColorKey(new Color(0.6f, 0.1f, 0f), 1f);    // Dark red
        
        GradientAlphaKey[] alphaKeys = new GradientAlphaKey[3];
        alphaKeys[0] = new GradientAlphaKey(1f, 0f);
        alphaKeys[1] = new GradientAlphaKey(0.8f, 0.5f);
        alphaKeys[2] = new GradientAlphaKey(0f, 1f);
        
        gradient.SetKeys(colorKeys, alphaKeys);
        colorOverLifetime.color = new ParticleSystem.MinMaxGradient(gradient);
        
        // Size over lifetime
        sizeOverLifetime.enabled = true;
        AnimationCurve sizeCurve = new AnimationCurve();
        sizeCurve.AddKey(0f, 1f);
        sizeCurve.AddKey(0.5f, 1.1f);
        sizeCurve.AddKey(1f, 0.1f);
        sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, sizeCurve);
        
        // Velocity - add some wavering
        velocityOverLifetime.enabled = true;
        velocityOverLifetime.space = ParticleSystemSimulationSpace.World;
        velocityOverLifetime.x = new ParticleSystem.MinMaxCurve(-0.3f, 0.3f);
        velocityOverLifetime.z = new ParticleSystem.MinMaxCurve(-0.3f, 0.3f);
        
        // Renderer
        renderer.renderMode = ParticleSystemRenderMode.Billboard;
        renderer.material = CreateFireMaterial();
        
        // Start stopped
        fireParticles.Stop();
    }
    
    void CreateSparkParticles()
    {
        GameObject sparkObj = new GameObject("TorchSparks");
        sparkObj.transform.SetParent(transform);
        sparkObj.transform.localPosition = Vector3.zero;
        sparkObj.transform.localRotation = Quaternion.identity;
        
        sparkParticles = sparkObj.AddComponent<ParticleSystem>();
        
        var main = sparkParticles.main;
        var emission = sparkParticles.emission;
        var shape = sparkParticles.shape;
        var colorOverLifetime = sparkParticles.colorOverLifetime;
        var sizeOverLifetime = sparkParticles.sizeOverLifetime;
        var renderer = sparkParticles.GetComponent<ParticleSystemRenderer>();
        
        // Main module
        main.startLifetime = new ParticleSystem.MinMaxCurve(0.3f, 0.6f);
        main.startSpeed = new ParticleSystem.MinMaxCurve(0.5f, 2f);
        main.startSize = new ParticleSystem.MinMaxCurve(sparkSize * 0.5f, sparkSize);
        main.maxParticles = 30;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.gravityModifier = 0.2f; // Slight gravity
        
        // Emission
        emission.rateOverTime = sparkEmissionRate;
        
        // Shape
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Circle;
        shape.radius = emissionRadius * 0.5f;
        
        // Color - bright sparks
        colorOverLifetime.enabled = true;
        Gradient gradient = new Gradient();
        
        GradientColorKey[] colorKeys = new GradientColorKey[2];
        colorKeys[0] = new GradientColorKey(new Color(1f, 1f, 0.5f), 0f);
        colorKeys[1] = new GradientColorKey(new Color(1f, 0.5f, 0f), 1f);
        
        GradientAlphaKey[] alphaKeys = new GradientAlphaKey[2];
        alphaKeys[0] = new GradientAlphaKey(1f, 0f);
        alphaKeys[1] = new GradientAlphaKey(0f, 1f);
        
        gradient.SetKeys(colorKeys, alphaKeys);
        colorOverLifetime.color = new ParticleSystem.MinMaxGradient(gradient);
        
        // Size over lifetime
        sizeOverLifetime.enabled = true;
        AnimationCurve sizeCurve = new AnimationCurve();
        sizeCurve.AddKey(0f, 1f);
        sizeCurve.AddKey(1f, 0.2f);
        sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, sizeCurve);
        
        // Renderer
        renderer.renderMode = ParticleSystemRenderMode.Billboard;
        renderer.material = CreateFireMaterial();
        
        // Start stopped
        sparkParticles.Stop();
    }
    
    void CreateTorchLight()
    {
        GameObject lightObj = new GameObject("TorchLight");
        lightObj.transform.SetParent(transform);
        lightObj.transform.localPosition = new Vector3(0, 0.2f, 0);
        
        torchLight = lightObj.AddComponent<Light>();
        torchLight.type = LightType.Point;
        torchLight.color = lightColor;
        torchLight.intensity = lightIntensity;
        torchLight.range = lightRange;
        torchLight.shadows = castShadows ? LightShadows.Soft : LightShadows.None;
        torchLight.enabled = false;
    }
    
    void CreateAudioSource()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.loop = true;
        audioSource.volume = soundVolume;
        audioSource.spatialBlend = 1f; // 3D sound
        audioSource.maxDistance = lightRange;
        audioSource.rolloffMode = AudioRolloffMode.Linear;
        
        // Generate procedural fire sound
        GenerateFireSound();
    }
    
    void GenerateFireSound()
    {
        // Create a simple crackling fire sound using white noise
        int sampleRate = 44100;
        int duration = 2;
        int samples = sampleRate * duration;
        
        AudioClip clip = AudioClip.Create("FireCrackle", samples, 1, sampleRate, false);
        float[] data = new float[samples];
        
        for (int i = 0; i < samples; i++)
        {
            // Low frequency rumble with occasional pops
            float noise = Random.Range(-0.1f, 0.1f);
            float crackle = Random.value > 0.98f ? Random.Range(0.2f, 0.4f) : 0f;
            data[i] = noise + crackle;
        }
        
        clip.SetData(data, 0);
        audioSource.clip = clip;
    }
    
    Material CreateFireMaterial()
    {
        Material material = new Material(Shader.Find("Particles/Standard Unlit"));
        material.SetInt("_BlendMode", 1);
        material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.One);
        material.SetInt("_ZWrite", 0);
        material.EnableKeyword("_ALPHABLEND_ON");
        material.renderQueue = 3000;
        return material;
    }
    
    void Update()
    {
        if (isLit)
        {
            UpdateFlicker();
        }
    }
    
    void UpdateFlicker()
    {
        // Generate new flicker target periodically
        flickerTimer += Time.deltaTime * flickerSpeed;
        
        if (flickerTimer >= 1f)
        {
            flickerTimer = 0f;
            targetFlicker = 1f + Random.Range(-flickerAmount, flickerAmount * 0.5f);
        }
        
        // Smoothly interpolate to target flicker
        currentFlicker = Mathf.Lerp(currentFlicker, targetFlicker, Time.deltaTime * 10f);
        
        // Apply flicker to light
        if (torchLight != null)
        {
            torchLight.intensity = baseLightIntensity * currentFlicker;
        }
        
        // Apply slight flicker to particle emission
        if (fireParticles != null)
        {
            var emission = fireParticles.emission;
            emission.rateOverTime = emissionRate * currentFlicker;
        }
    }
    
    // Public methods
    public void LightTorch()
    {
        if (isLit) return;
        
        isLit = true;
        
        if (fireParticles != null)
        {
            fireParticles.Play();
        }
        
        if (sparkParticles != null && enableSparks)
        {
            sparkParticles.Play();
        }
        
        if (torchLight != null)
        {
            torchLight.enabled = true;
        }
        
        if (audioSource != null && enableSound)
        {
            audioSource.Play();
        }
    }
    
    public void ExtinguishTorch()
    {
        if (!isLit) return;
        
        isLit = false;
        
        if (fireParticles != null)
        {
            fireParticles.Stop();
        }
        
        if (sparkParticles != null)
        {
            sparkParticles.Stop();
        }
        
        if (torchLight != null)
        {
            torchLight.enabled = false;
        }
        
        if (audioSource != null)
        {
            audioSource.Stop();
        }
    }
    
    public void ToggleTorch()
    {
        if (isLit)
        {
            ExtinguishTorch();
        }
        else
        {
            LightTorch();
        }
    }
    
    public bool IsLit()
    {
        return isLit;
    }
    
    public void SetFlickerAmount(float amount)
    {
        flickerAmount = Mathf.Clamp01(amount);
    }
    
    public void SetFlickerSpeed(float speed)
    {
        flickerSpeed = Mathf.Max(0.1f, speed);
    }
}
