using UnityEngine;

[CreateAssetMenu(fileName = "Effect_GuardianBond_", menuName = "CircleOfShields/Effects/Guardian Bond")]
public class GuardianBondEffect : UpgradeEffect
{
    [SerializeField] private float damageMultiplier = 2f;
    
    public override void Apply()
    {
        GameObject mage = GameObject.FindGameObjectWithTag("Mage");
        if (mage == null) return;
        
        MageGirl mageGirl = mage.GetComponent<MageGirl>();
        if (mageGirl == null) return;
        
        mageGirl.EnableGuardianBond(damageMultiplier);
        Debug.Log("Guardian Bond activated: next shot after parry x" + damageMultiplier);
    }
}