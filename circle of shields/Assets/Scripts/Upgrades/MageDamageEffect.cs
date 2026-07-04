using UnityEngine;

[CreateAssetMenu(fileName = "Effect_MageDamage_", menuName = "CircleOfShields/Effects/Mage Damage")]
public class MageDamageEffect : UpgradeEffect
{
    [SerializeField] private int damageBonus = 5;
    
    public override void Apply()
    {
        GameObject mage = GameObject.FindGameObjectWithTag("Mage");
        if (mage == null) return;
        
        MageGirl mageGirl = mage.GetComponent<MageGirl>();
        if (mageGirl == null) return;
        
        mageGirl.AddDamage(damageBonus);
        Debug.Log("Mage damage +" + damageBonus);
    }
}