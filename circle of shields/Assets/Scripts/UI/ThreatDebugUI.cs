using UnityEngine;
using TMPro;

public class ThreatDebugUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI threatText;
    
    private void Update()
    {
        if (threatText == null) return;
        
        float threat = ThreatCalculator.CalculateCurrentThreat();
        string category = ThreatCalculator.GetThreatCategory(threat);
        
        threatText.text = "Threat: " + threat.ToString("F1") + " (" + category + ")";
        
        // Цвет по категории
        switch (category)
        {
            case "SAFE":       threatText.color = Color.green; break;
            case "MODERATE":   threatText.color = Color.yellow; break;
            case "DANGEROUS":  threatText.color = new Color(1, 0.5f, 0); break;
            case "CRITICAL":   threatText.color = Color.red; break;
        }
    }
}