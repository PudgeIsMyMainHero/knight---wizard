using UnityEngine;

[CreateAssetMenu(fileName = "Effect_KnightHP_", menuName = "CircleOfShields/Effects/Knight Max HP")]
public class KnightMaxHPEffect : UpgradeEffect
{
    [SerializeField] private int hpAmount = 30;
    
    public override void Apply()
    {
        GameObject knight = GameObject.FindGameObjectWithTag("Player");
        if (knight == null) return;
        
        Health health = knight.GetComponent<Health>();
        if (health == null) return;
        
        // Увеличиваем макс HP и лечим
        // Нужно добавить метод в Health для этого (см. ниже)
        health.IncreaseMaxHealth(hpAmount);
        health.Heal(hpAmount);
        
        Debug.Log("Knight max HP +" + hpAmount);
    }
}