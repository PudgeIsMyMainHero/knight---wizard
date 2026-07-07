using UnityEngine;
using TMPro;

public class HealthNumberUI : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Health targetHealth;
    
    [Header("UI")]
    [SerializeField] private TextMeshProUGUI healthText;
    
    [Header("Display")]
    [SerializeField] private Color textColor = Color.white;
    [SerializeField] private string separator = " / ";
    
    private void Start()
    {
        if (healthText != null)
            healthText.color = textColor;
        
        if (targetHealth != null)
        {
            targetHealth.OnHealthChanged.AddListener(UpdateText);
            UpdateText(targetHealth.CurrentHealth, targetHealth.MaxHealth);
        }
    }
    
    private void UpdateText(int current, int max)
    {
        if (healthText != null)
            healthText.text = current + separator + max;
    }
    
    private void OnDestroy()
    {
        if (targetHealth != null)
            targetHealth.OnHealthChanged.RemoveListener(UpdateText);
    }
}