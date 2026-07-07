using UnityEngine;

[CreateAssetMenu(fileName = "Effect_HealBoth_", menuName = "CircleOfShields/Effects/Heal Both")]
public class HealBothEffect : UpgradeEffect
{
    [SerializeField] private int healAmount = 40;
    [SerializeField] private bool fullHeal = false;
    
    public override void Apply()
    {
        HealTarget("Player");
        HealTarget("Mage");
    }
    
    private void HealTarget(string tag)
    {
        GameObject target = GameObject.FindGameObjectWithTag(tag);
        if (target == null) return;
        
        Health health = target.GetComponent<Health>();
        if (health == null) return;
        
        if (fullHeal)
            health.Heal(health.MaxHealth - health.CurrentHealth);
        else
            health.Heal(healAmount);
        
        Debug.Log(tag + " healed by " + (fullHeal ? "FULL" : healAmount.ToString()));
    }
}