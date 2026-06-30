using UnityEngine;

public static class DeathMarkEffect
{
    public static void Trigger(GameObject target, Vector2 hitPosition)
    {
        if (target == null) return;
        
        // Накладываем метку
        DeathMark mark = target.GetComponent<DeathMark>();
        if (mark == null)
            mark = target.AddComponent<DeathMark>();
        
        mark.ApplyMark(1.5f);
        
        // Визуальная вспышка в точке попадания
        ParryEffect.SpawnFlash(hitPosition);
        
        // Девочка немедленно делает усиленный выстрел по цели
        MageGirl mage = Object.FindObjectOfType<MageGirl>();
        if (mage != null)
            mage.ForcedAttack(target.transform);
        
        Debug.Log("DEATH MARK applied! Mage empowered attack incoming!");
    }
}