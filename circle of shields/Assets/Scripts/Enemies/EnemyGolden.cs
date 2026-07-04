using UnityEngine;

/// Компонент делающий врага "золотым" — при парри его снаряда обратно в него
/// игрок получает выбор апгрейдов
public class EnemyGolden : MonoBehaviour
{
    [Header("Visual")]
    [SerializeField] private Color goldColor = new Color(1f, 0.85f, 0.2f, 1f);
    [SerializeField] private float pulseSpeed = 2f;
    [SerializeField] private float pulseIntensity = 0.3f;
    
    [Header("Glow")]
    [SerializeField] private bool spawnGlowEffect = true;
    [SerializeField] private Color glowColor = new Color(1f, 0.85f, 0.2f, 0.4f);
    [SerializeField] private float glowSize = 1.5f;
    
    private SpriteRenderer spriteRenderer;
    private Color baseColor;
    private GameObject glowVisual;
    private bool triggered = false;
    
    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
            baseColor = spriteRenderer.color;
    }
    
    private void Start()
    {
        ApplyGoldenAppearance();
        if (spawnGlowEffect)
            CreateGlow();
    }
    
    private void Update()
    {
        if (triggered) return;
        
        // Пульсация для привлечения внимания
        if (spriteRenderer != null)
        {
            float pulse = 1f + Mathf.Sin(Time.time * pulseSpeed) * pulseIntensity;
            spriteRenderer.color = new Color(
                goldColor.r * pulse,
                goldColor.g * pulse,
                goldColor.b * pulse,
                goldColor.a
            );
        }
        
        // Пульсация glow
        if (glowVisual != null)
        {
            float glowPulse = 1f + Mathf.Sin(Time.time * pulseSpeed) * 0.2f;
            glowVisual.transform.localScale = Vector3.one * glowSize * glowPulse;
        }
    }
    
    private void ApplyGoldenAppearance()
    {
        if (spriteRenderer != null)
            spriteRenderer.color = goldColor;
    }
    
    private void CreateGlow()
    {
        glowVisual = new GameObject("GoldenGlow");
        glowVisual.transform.SetParent(transform);
        glowVisual.transform.localPosition = Vector3.zero;
        glowVisual.transform.localScale = Vector3.one * glowSize;
        
        SpriteRenderer sr = glowVisual.AddComponent<SpriteRenderer>();
        sr.sprite = CreateCircleSprite();
        sr.color = glowColor;
        sr.sortingOrder = -1; // За врагом
    }
    
    /// Вызывается когда отражённый снаряд попал в этого врага
    public void OnGoldenHit()
    {
        if (triggered) return;
        triggered = true;
        
        Debug.Log("★ GOLDEN ENEMY HIT! Upgrade offer!");
        
        // Убираем визуальные эффекты
        if (glowVisual != null)
            Destroy(glowVisual);
        
        // Триггерим выбор апгрейдов
        if (UpgradeManager.Instance != null)
            UpgradeManager.Instance.OfferUpgrades();
        
        // Убиваем врага (он выполнил свою функцию)
        Health health = GetComponent<Health>();
        if (health != null)
            health.TakeDamage(9999, "Golden Enemy Sacrifice");
        else
            Destroy(gameObject);
    }
    
    private Sprite CreateCircleSprite()
    {
        int size = 64;
        Texture2D texture = new Texture2D(size, size);
        texture.filterMode = FilterMode.Bilinear;
        
        Vector2 center = new Vector2(size / 2f, size / 2f);
        float radius = size / 2f;
        
        for (int x = 0; x < size; x++)
            for (int y = 0; y < size; y++)
            {
                float dist = Vector2.Distance(new Vector2(x, y), center);
                float alpha = dist < radius ? 1f - (dist / radius) * 0.7f : 0f;
                texture.SetPixel(x, y, new Color(1, 1, 1, alpha));
            }
        
        texture.Apply();
        return Sprite.Create(texture, new Rect(0, 0, size, size), Vector2.one * 0.5f, size);
    }
    
    private void OnDestroy()
    {
        if (glowVisual != null)
            Destroy(glowVisual);
    }
}