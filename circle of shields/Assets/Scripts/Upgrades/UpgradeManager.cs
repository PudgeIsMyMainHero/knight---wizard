using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public class UpgradeManager : MonoBehaviour
{
    public static UpgradeManager Instance { get; private set; }
    
    [Header("Available Upgrades")]
    [Tooltip("Все возможные апгрейды в игре")]
    [SerializeField] private List<UpgradeData> allUpgrades = new List<UpgradeData>();
    
    [Header("Settings")]
    [SerializeField] private int choicesPerOffer = 3;
    
    // Счётчик взятых апгрейдов
    private Dictionary<UpgradeData, int> takenUpgrades = new Dictionary<UpgradeData, int>();
    
    // Событие для UI: список доступных апгрейдов на выбор
    public UnityEvent<List<UpgradeData>> OnUpgradesOffered;
    
    // Событие: выбран апгрейд
    public UnityEvent<UpgradeData> OnUpgradeApplied;
    
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }
    
    /// Получить случайный список апгрейдов для выбора
    public List<UpgradeData> GetRandomChoices()
    {
        List<UpgradeData> available = allUpgrades
            .Where(u => u != null && CanTakeMore(u))
            .ToList();
        
        if (available.Count == 0)
        {
            Debug.Log("No more upgrades available!");
            return new List<UpgradeData>();
        }
        
        List<UpgradeData> choices = new List<UpgradeData>();
        int count = Mathf.Min(choicesPerOffer, available.Count);
        
        for (int i = 0; i < count; i++)
        {
            UpgradeData picked = PickWeightedRandom(available);
            if (picked != null)
            {
                choices.Add(picked);
                available.Remove(picked); // Убираем чтобы не повторялось
            }
        }
        
        return choices;
    }
    
    /// Показать выбор апгрейдов игроку (через событие)
    public void OfferUpgrades()
    {
        List<UpgradeData> choices = GetRandomChoices();
        
        if (choices.Count == 0)
        {
            Debug.Log("No upgrades to offer");
            return;
        }
        
        Debug.Log("Offering " + choices.Count + " upgrades");
        
        // Пауза
        Time.timeScale = 0f;
        
        OnUpgradesOffered?.Invoke(choices);
    }
    
    public void ApplyUpgrade(UpgradeData upgrade)
    {
        if (upgrade == null) return;
        
        upgrade.Apply();
        
        if (!takenUpgrades.ContainsKey(upgrade))
            takenUpgrades[upgrade] = 0;
        takenUpgrades[upgrade]++;
        
        Debug.Log("Applied upgrade: " + upgrade.upgradeName + 
                  " (stack " + takenUpgrades[upgrade] + "/" + upgrade.maxStacks + ")");
        
        // Возобновляем время
        Time.timeScale = 1f;
        
        OnUpgradeApplied?.Invoke(upgrade);
    }
    
    /// Взвешенный случайный выбор
    private UpgradeData PickWeightedRandom(List<UpgradeData> pool)
    {
        if (pool.Count == 0) return null;
        
        float totalWeight = pool.Sum(u => u.weight);
        float random = Random.Range(0f, totalWeight);
        float cumulative = 0f;
        
        foreach (UpgradeData upgrade in pool)
        {
            cumulative += upgrade.weight;
            if (random <= cumulative)
                return upgrade;
        }
        
        return pool[pool.Count - 1];
    }
    
    /// Можно ли ещё взять этот апгрейд
    private bool CanTakeMore(UpgradeData upgrade)
    {
        if (!takenUpgrades.ContainsKey(upgrade))
            return true;
        
        return takenUpgrades[upgrade] < upgrade.maxStacks;
    }
    
    /// Получить количество стаков конкретного апгрейда
    public int GetStackCount(UpgradeData upgrade)
    {
        return takenUpgrades.ContainsKey(upgrade) ? takenUpgrades[upgrade] : 0;
    }
    
    /// Сбросить все апгрейды (например при рестарте)
    public void ResetAllUpgrades()
    {
        takenUpgrades.Clear();
    }
    
    
}