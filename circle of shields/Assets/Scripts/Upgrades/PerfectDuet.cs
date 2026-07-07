using UnityEngine;

[CreateAssetMenu(fileName = "Effect_PerfectDuet_", menuName = "CircleOfShields/Effects/Perfect Duet")]
public class PerfectDuetEffect : UpgradeEffect
{
    [SerializeField] private int healAmount = 5;
    
    public override void Apply()
    {
        // Активируем эффект глобально
        PerfectDuetManager.Instance.Activate(healAmount);
        Debug.Log("Perfect Duet activated: +" + healAmount + " HP on each parry");
    }
}