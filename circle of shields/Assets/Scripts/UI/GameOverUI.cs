using UnityEngine;
using TMPro;

public class GameOverUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI reasonText;
    
    public void SetReason(string reason)
    {
        if (reasonText != null)
            reasonText.text = reason;
    }
}