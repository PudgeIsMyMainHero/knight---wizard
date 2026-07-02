using UnityEngine;

public class Slowable : MonoBehaviour
{
    [Header("Freeze on Second Hit")]
    [SerializeField] private float freezeDuration = 2f;
    [SerializeField] private float stackWindow = 3f;
    
    // State
    private float slowTimer = 0f;
    private float freezeTimer = 0f;
    private float lastHitTime = -999f;
    private bool isFrozen = false;
    
    // Visual
    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    private bool colorSaved = false;
    
    // Public
    public float CurrentSpeedMultiplier { get; private set; } = 1f;
    public bool IsFrozen => isFrozen;
    
    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }
    
    private void Update()
    {
        // Заморозка
        if (isFrozen)
        {
            freezeTimer -= Time.deltaTime;
            if (freezeTimer <= 0)
                Unfreeze();
            return;
        }
        
        // Замедление
        if (slowTimer > 0)
        {
            slowTimer -= Time.deltaTime;
            if (slowTimer <= 0)
                RemoveSlow();
        }
    }
    
    // Обычное замедление (без стакания)
    public void ApplySlow(float multiplier, float duration)
    {
        if (isFrozen) return;
        
        SaveColor();
        
        CurrentSpeedMultiplier = Mathf.Min(CurrentSpeedMultiplier == 1f ? multiplier : CurrentSpeedMultiplier, multiplier);
        slowTimer = Mathf.Max(slowTimer, duration);
        
        UpdateVisual();
    }
    
    // Замедление с накоплением (2-й hit = заморозка)
    public void ApplyStackingFrost(float slowMultiplier, float slowDuration)
    {
        if (isFrozen) return;
        
        SaveColor();
        
        float timeSinceLastHit = Time.time - lastHitTime;
        
        if (timeSinceLastHit < stackWindow)
        {
            // 2-й hit — ЗАМОРОЗКА
            Freeze(freezeDuration);
            Debug.Log(gameObject.name + " FROZEN!");
        }
        else
        {
            // 1-й hit — обычное замедление
            ApplySlow(slowMultiplier, slowDuration);
            Debug.Log(gameObject.name + " slowed (1st stack)");
        }
        
        lastHitTime = Time.time;
    }
    
    public void Freeze(float duration)
    {
        SaveColor();
        
        isFrozen = true;
        freezeTimer = duration;
        CurrentSpeedMultiplier = 0f;
        
        UpdateVisual();
    }
    
    private void Unfreeze()
    {
        isFrozen = false;
        CurrentSpeedMultiplier = 1f;
        slowTimer = 0f;
        
        RestoreColor();
    }
    
    private void RemoveSlow()
    {
        CurrentSpeedMultiplier = 1f;
        RestoreColor();
    }
    
    private void SaveColor()
    {
        if (colorSaved) return;
        if (spriteRenderer == null) return;
        
        originalColor = spriteRenderer.color;
        colorSaved = true;
    }
    
    private void RestoreColor()
    {
        if (spriteRenderer != null && colorSaved)
            spriteRenderer.color = originalColor;
    }
    
    private void UpdateVisual()
    {
        if (spriteRenderer == null) return;
        
        if (isFrozen)
            spriteRenderer.color = new Color(0.6f, 0.9f, 1f, 1f); // Яркий лёд
        else
            spriteRenderer.color = Color.Lerp(originalColor, new Color(0.5f, 0.8f, 1f, 1f), 0.6f);
    }
}