using UnityEngine;

[CreateAssetMenu(fileName = "Effect_KnightSpeed_", menuName = "CircleOfShields/Effects/Knight Speed")]
public class KnightSpeedEffect : UpgradeEffect
{
    [SerializeField] private float speedBonus = 1f;
    
    public override void Apply()
    {
        GameObject knight = GameObject.FindGameObjectWithTag("Player");
        if (knight == null) return;
        
        KnightController controller = knight.GetComponent<KnightController>();
        if (controller == null) return;
        
        controller.AddMoveSpeed(speedBonus);
        Debug.Log("Knight speed +" + speedBonus);
    }
}