using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UpgradeCard : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private Image iconImage;
    [SerializeField] private Image backgroundImage;
    [SerializeField] private Image rarityBorder;
    [SerializeField] private Button selectButton;
    
    [Header("Rarity Colors")]
    [SerializeField] private Color commonColor = new Color(0.7f, 0.7f, 0.7f, 1f);
    [SerializeField] private Color rareColor = new Color(0.4f, 0.6f, 1f, 1f);
    [SerializeField] private Color epicColor = new Color(0.7f, 0.3f, 1f, 1f);
    
    private UpgradeData upgradeData;
    private System.Action<UpgradeData> onSelected;
    
    public void Setup(UpgradeData data, System.Action<UpgradeData> onSelectCallback)
    {
        upgradeData = data;
        onSelected = onSelectCallback;
        
        if (nameText != null)
            nameText.text = data.upgradeName;
        
        if (descriptionText != null)
            descriptionText.text = data.description;
        
        if (iconImage != null && data.icon != null)
        {
            iconImage.sprite = data.icon;
            iconImage.enabled = true;
        }
        else if (iconImage != null)
        {
            iconImage.enabled = false;
        }
        
        // Цвет по редкости
        Color rarityColor = GetRarityColor(data.rarity);
        if (rarityBorder != null)
            rarityBorder.color = rarityColor;
        
        // Показать стаки
        if (UpgradeManager.Instance != null)
        {
            int stacks = UpgradeManager.Instance.GetStackCount(data);
            if (stacks > 0 && descriptionText != null)
            {
                descriptionText.text += "\n<color=#888888>[" + stacks + "/" + data.maxStacks + "]</color>";
            }
        }
        
        // Кнопка
        if (selectButton != null)
        {
            selectButton.onClick.RemoveAllListeners();
            selectButton.onClick.AddListener(OnClick);
        }
    }
    
    private void OnClick()
    {
        if (onSelected != null && upgradeData != null)
            onSelected.Invoke(upgradeData);
    }
    
    private Color GetRarityColor(UpgradeRarity rarity)
    {
        switch (rarity)
        {
            case UpgradeRarity.Common: return commonColor;
            case UpgradeRarity.Rare:   return rareColor;
            case UpgradeRarity.Epic:   return epicColor;
            default:                    return commonColor;
        }
    }
}