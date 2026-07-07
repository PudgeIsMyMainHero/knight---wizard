using UnityEngine;

public class ProjectileParryHint : MonoBehaviour
{
    [Header("Detection")]
    [SerializeField] private Transform knightTransform;
    [SerializeField] private float hintDistance = 5f;   // увеличено
    
    [Header("Visual")]
    [SerializeField] private SpriteRenderer targetRenderer;
    [SerializeField] private Color hintColor = new Color(1f, 1f, 0.5f, 1f);   // жёлтый
    [SerializeField] private float pulseSpeed = 15f;
    [SerializeField] private float pulseIntensity = 0.7f;   // усилено
    
    private Color originalColor;
    private Projectile projectile;
    private bool isHinting = false;
    private bool colorSaved = false;
    
    private void Awake()
    {
        // Ищем в дочерних тоже
        if (targetRenderer == null)
            targetRenderer = GetComponentInChildren<SpriteRenderer>();
        
        projectile = GetComponent<Projectile>();
    }
    
    private void Start()
    {
        // Сохраняем цвет ПОСЛЕ Initialize
        if (targetRenderer != null && !colorSaved)
        {
            originalColor = targetRenderer.color;
            colorSaved = true;
        }
        
        // Ищем рыцаря
        if (knightTransform == null)
        {
            GameObject knight = GameObject.FindGameObjectWithTag("Player");
            if (knight != null)
                knightTransform = knight.transform;
        }
    }
    
    private void Update()
    {
        if (targetRenderer == null || knightTransform == null) return;
        
        // Отражённый снаряд — не пульсируем
        if (projectile != null && projectile.IsReflected)
        {
            RestoreColor();
            return;
        }
        
        float distance = Vector2.Distance(transform.position, knightTransform.position);
        
        if (distance <= hintDistance)
        {
            if (!isHinting)
            {
                isHinting = true;
                // Пересохраняем цвет на случай изменений
                originalColor = targetRenderer.color;
            }
            
            ApplyPulse(distance);
        }
        else if (isHinting)
        {
            RestoreColor();
            isHinting = false;
        }
    }
    
    private void ApplyPulse(float distance)
    {
        // Чем ближе — тем ярче
        float proximity = 1f - (distance / hintDistance);
        proximity = Mathf.Clamp01(proximity);
        
        // Пульсация
        float pulse = 0.5f + Mathf.Sin(Time.time * pulseSpeed) * 0.5f;
        pulse *= proximity;
        pulse = Mathf.Clamp01(pulse);
        
        Color mixed = Color.Lerp(originalColor, hintColor, pulse * pulseIntensity);
        targetRenderer.color = mixed;
    }
    
    private void RestoreColor()
    {
        if (targetRenderer != null && colorSaved)
            targetRenderer.color = originalColor;
    }
    
    private void OnDestroy()
    {
        RestoreColor();
    }
}