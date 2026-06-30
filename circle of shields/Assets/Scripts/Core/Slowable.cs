using UnityEngine;

public class Slowable : MonoBehaviour
{
    private float originalTimeScale = 1f;
    private float slowTimer = 0f;
    private bool isSlowed = false;
    
    public float CurrentSpeedMultiplier { get; private set; } = 1f;
    
    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    private bool colorSaved = false;
    
    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }
    
    private void Update()
    {
        if (isSlowed)
        {
            slowTimer -= Time.deltaTime;
            if (slowTimer <= 0)
                RemoveSlow();
        }
    }
    
    public void ApplySlow(float multiplier, float duration)
    {
        if (!colorSaved && spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
            colorSaved = true;
        }
        
        // Если уже замедлен — продлеваем время, берём более сильный эффект
        CurrentSpeedMultiplier = Mathf.Min(CurrentSpeedMultiplier == 1f ? multiplier : CurrentSpeedMultiplier, multiplier);
        slowTimer = Mathf.Max(slowTimer, duration);
        isSlowed = true;
        
        // Визуал — голубой оттенок
        if (spriteRenderer != null)
            spriteRenderer.color = Color.Lerp(originalColor, new Color(0.5f, 0.8f, 1f, 1f), 0.6f);
    }
    
    private void RemoveSlow()
    {
        isSlowed = false;
        CurrentSpeedMultiplier = 1f;
        
        if (spriteRenderer != null && colorSaved)
            spriteRenderer.color = originalColor;
    }
}