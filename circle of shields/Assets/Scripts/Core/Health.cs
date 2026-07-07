using UnityEngine;
using UnityEngine.Events;

public class Health : MonoBehaviour
{
    [Header("Health Settings")]
    [SerializeField] private int maxHealth = 100;
    [SerializeField] private int currentHealth;
    
    [Header("Events")]
    public UnityEvent<int, int> OnHealthChanged;
    public UnityEvent OnDeath;
    
    // Неуязвимость
    private bool isInvulnerable = false;
    
    // Properties
    public int CurrentHealth => currentHealth;
    public int MaxHealth => maxHealth;
    public bool IsDead => currentHealth <= 0;
    public bool IsInvulnerable => isInvulnerable;
    
    private void Awake()
    {
        currentHealth = maxHealth;
    }
    
    public void TakeDamage(int damage, string source = "Unknown")
    {
        if (IsDead) return;
        if (isInvulnerable) return;
        
        int finalDamage = damage;
        
        // Проверка метки смерти
        DeathMark mark = GetComponent<DeathMark>();
        if (mark != null && mark.IsMarked)
        {
            finalDamage = Mathf.RoundToInt(damage * mark.BonusDamageMultiplier);
            mark.ConsumeMark();
            Debug.Log(gameObject.name + " MARK CONSUMED! Bonus damage: " + finalDamage);
        }
        
        
        currentHealth -= finalDamage;
        currentHealth = Mathf.Max(0, currentHealth);
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
        
        if (IsDead)
        {
            OnDeath?.Invoke();
            Debug.Log(gameObject.name + " DIED! Killed by [" + source + "]");
        }
    }
    
    public void Heal(int amount)
    {
        if (IsDead) return;
        
        currentHealth += amount;
        currentHealth = Mathf.Min(currentHealth, maxHealth);
        
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }
    
    public void SetInvulnerable(bool value)
    {
        isInvulnerable = value;
    }
    
    public void IncreaseMaxHealth(int amount)
    {
        maxHealth += amount;
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }
}