using UnityEngine;

public class ShockwaveParticleEffect : MonoBehaviour
{
    [Header("Appearance")]
    public Color innerColor = new Color(0.2f, 0.4f, 1f, 0.8f);
    public Color outerColor = new Color(0.2f, 0.4f, 1f, 0f);
    public float particleSize = 0.3f;
    public float particleCount = 150;
    
    [Header("Animation")]
    public float expansionSpeed = 8f;
    public float maxRadius = 5f;
    public float fadeOutTime = 0.5f;
    
    private ParticleSystem shockwavePS;
    private ParticleSystemRenderer particleRenderer;
    
    void Awake()
    {
        Setup();
    }
    
    void Setup()
    {
        // Get or create particle system
        shockwavePS = GetComponent<ParticleSystem>();
        if (shockwavePS == null)
        {
            shockwavePS = gameObject.AddComponent<ParticleSystem>();
        }
        
        particleRenderer = GetComponent<ParticleSystemRenderer>();
        
        // Configure the particle system
        ConfigureParticleSystem();
        
        // Configure the renderer
        ConfigureRenderer();
    }
    
    void ConfigureParticleSystem()
    {
        // Main module configuration
        var main = shockwavePS.main;
        main.duration = maxRadius / expansionSpeed;
        main.loop = false;
        main.startLifetime = main.duration + fadeOutTime;
        main.startSpeed = expansionSpeed;
        main.startSize = particleSize;
        main.startColor = innerColor;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.playOnAwake = true;
        main.maxParticles = 500;
        
        // Emission module configuration
        var emission = shockwavePS.emission;
        emission.enabled = true;
        emission.rateOverTime = 0;
        emission.SetBursts(new ParticleSystem.Burst[] { 
            new ParticleSystem.Burst(0.0f, particleCount) 
        });
        
        // Shape module - donut/ring
        var shape = shockwavePS.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Donut;
        shape.radius = 0.2f;  // Initial small radius for the donut
        shape.radiusThickness = 0;  // Emit from surface only
        shape.arc = 360f;  // Full circle
        
        // Size over lifetime
        var sizeOverLifetime = shockwavePS.sizeOverLifetime;
        sizeOverLifetime.enabled = true;
        AnimationCurve sizeOverLifetimeCurve = new AnimationCurve();
        sizeOverLifetimeCurve.AddKey(0.0f, 1.0f);
        sizeOverLifetimeCurve.AddKey(1.0f, 2.0f);
        sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, sizeOverLifetimeCurve);
        
        // Color over lifetime - fade out
        var colorOverLifetime = shockwavePS.colorOverLifetime;
        colorOverLifetime.enabled = true;
        Gradient colorGradient = new Gradient();
        colorGradient.SetKeys(
            new GradientColorKey[] { 
                new GradientColorKey(innerColor, 0.0f),
                new GradientColorKey(outerColor, 1.0f) 
            },
            new GradientAlphaKey[] { 
                new GradientAlphaKey(1.0f, 0.0f), 
                new GradientAlphaKey(0.7f, 0.7f),
                new GradientAlphaKey(0.0f, 1.0f) 
            }
        );
        colorOverLifetime.color = colorGradient;
        
        // Velocity over lifetime - for slight upward movement
        var velocityOverLifetime = shockwavePS.velocityOverLifetime;
        velocityOverLifetime.enabled = true;
        velocityOverLifetime.y = 0.5f;  // Slight upward drift
    }
    
    void ConfigureRenderer()
    {
        particleRenderer.renderMode = ParticleSystemRenderMode.Billboard;
        
        // Create material for the particles
        Material particleMaterial = new Material(Shader.Find("Particles/Standard Unlit"));
        particleMaterial.SetColor("_Color", innerColor);
        particleRenderer.material = particleMaterial;
        
        // Add a glow/bloom if your project uses post-processing
        particleMaterial.SetFloat("_EmissionStrength", 1.5f);
    }
    
    public void Play()
    {
        shockwavePS.Play();
    }
    
    // You can call this to customize the effect at runtime
    public void SetParameters(float radius, float speed, Color color)
    {
        maxRadius = radius;
        expansionSpeed = speed;
        innerColor = color;
        outerColor = new Color(color.r, color.g, color.b, 0);
        
        // Update particle system parameters
        var main = shockwavePS.main;
        main.duration = maxRadius / expansionSpeed;
        main.startLifetime = main.duration + fadeOutTime;
        main.startSpeed = expansionSpeed;
        main.startColor = innerColor;
        
        // Update color gradient
        var colorOverLifetime = shockwavePS.colorOverLifetime;
        Gradient colorGradient = new Gradient();
        colorGradient.SetKeys(
            new GradientColorKey[] { 
                new GradientColorKey(innerColor, 0.0f),
                new GradientColorKey(outerColor, 1.0f) 
            },
            new GradientAlphaKey[] { 
                new GradientAlphaKey(1.0f, 0.0f), 
                new GradientAlphaKey(0.7f, 0.7f),
                new GradientAlphaKey(0.0f, 1.0f) 
            }
        );
        colorOverLifetime.color = colorGradient;
    }
}