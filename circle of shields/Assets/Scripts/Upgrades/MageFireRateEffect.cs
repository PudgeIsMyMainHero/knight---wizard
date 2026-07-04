using UnityEngine;

[CreateAssetMenu(fileName = "Effect_MageFireRate_", menuName = "CircleOfShields/Effects/Mage Fire Rate")]
public class MageFireRateEffect : UpgradeEffect
{
    [Range(0.1f, 0.9f)]
    [SerializeField] private float intervalMultiplier = 0.8f;
    
    public override void Apply()
    {
        GameObject mage = GameObject.FindGameObjectWithTag("Mage");
        if (mage == null) return;
        
        MageGirl mageGirl = mage.GetComponent<MageGirl>();
        if (mageGirl == null) return;
        
        mageGirl.MultiplyShootInterval(intervalMultiplier);
        Debug.Log("Mage fire rate improved (x" + intervalMultiplier + ")");
    }
}