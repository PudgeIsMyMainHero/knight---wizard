using UnityEngine;

public class PerfectDuetManager : MonoBehaviour
{
    public static PerfectDuetManager Instance { get; private set; }
    
    private bool isActive = false;
    private int healAmount = 5;
    
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }
    
    public void Activate(int amount)
    {
        isActive = true;
        healAmount = Mathf.Max(healAmount, amount);   // если стакается — берём больший
    }
    
    public void OnParry()
    {
        if (!isActive) return;
        
        HealTarget("Player");
        HealTarget("Mage");
    }
    
    private void HealTarget(string tag)
    {
        GameObject target = GameObject.FindGameObjectWithTag(tag);
        if (target == null) return;
        
        Health health = target.GetComponent<Health>();
        if (health != null)
            health.Heal(healAmount);
    }
}