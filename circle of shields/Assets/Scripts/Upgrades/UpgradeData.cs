using UnityEngine;

public enum UpgradeRarity { Common, Rare, Epic }
public enum UpgradeCategory { Knight, Mage, Synergy, Special }

[CreateAssetMenu(fileName = "Upgrade_", menuName = "CircleOfShields/Upgrade")]
public class UpgradeData : ScriptableObject
{
    [Header("Display")]
    public string upgradeName = "New Upgrade";
    [TextArea(2, 4)]
    public string description = "Description of the upgrade";
    public Sprite icon;
    
    [Header("Category")]
    public UpgradeCategory category = UpgradeCategory.Knight;
    public UpgradeRarity rarity = UpgradeRarity.Common;
    
    [Header("Effect")]
    public UpgradeEffect effect;
    
    [Header("Stacking")]
    [Tooltip("Максимальное количество раз можно взять этот апгрейд")]
    public int maxStacks = 3;
    
    [Header("Weight")]
    [Tooltip("Вес для случайного выбора (выше = чаще)")]
    public float weight = 1f;
    
    /// Применить эффект апгрейда
    public void Apply()
    {
        if (effect != null)
            effect.Apply();
        else
            Debug.LogWarning("UpgradeData '" + upgradeName + "' has no effect!");
    }
}