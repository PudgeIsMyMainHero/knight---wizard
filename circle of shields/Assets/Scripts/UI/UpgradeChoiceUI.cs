using System.Collections.Generic;
using UnityEngine;

public class UpgradeChoiceUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject panel;
    [SerializeField] private Transform cardsContainer;
    [SerializeField] private GameObject cardPrefab;
    
    [Header("Optional")]
    [SerializeField] private UnityEngine.UI.Button skipButton;
    
    private List<GameObject> currentCards = new List<GameObject>();
    
    private void Start()
    {
        // Скрываем панель по умолчанию
        if (panel != null)
            panel.SetActive(false);
        
        // Подписываемся на события
        if (UpgradeManager.Instance != null)
        {
            UpgradeManager.Instance.OnUpgradesOffered.AddListener(ShowUpgrades);
        }
    }
    
    public void ShowUpgrades(List<UpgradeData> upgrades)
    {
        if (panel == null) return;
        
        panel.SetActive(true);
        
        if (DialogueSystem.Instance != null)
            DialogueSystem.Instance.BlockDialogue(true);
        
        // Очищаем старые карточки
        ClearCards();
        
        // Создаём новые
        foreach (UpgradeData upgrade in upgrades)
        {
            if (cardPrefab == null) continue;
            
            GameObject cardObj = Instantiate(cardPrefab, cardsContainer);
            currentCards.Add(cardObj);
            
            UpgradeCard card = cardObj.GetComponent<UpgradeCard>();
            if (card != null)
                card.Setup(upgrade, OnUpgradeSelected);
        }
    }
    
    private void OnUpgradeSelected(UpgradeData upgrade)
    {
        Debug.Log("Player selected: " + upgrade.upgradeName);
        
        if (UpgradeManager.Instance != null)
            UpgradeManager.Instance.ApplyUpgrade(upgrade);
        
        HidePanel();
    }
    
    private void HidePanel()
    {
        ClearCards();
        if (panel != null)
            panel.SetActive(false);
        if (DialogueSystem.Instance != null)
            DialogueSystem.Instance.BlockDialogue(false);
    }
    
    private void ClearCards()
    {
        foreach (GameObject card in currentCards)
        {
            if (card != null)
                Destroy(card);
        }
        currentCards.Clear();
    }
}