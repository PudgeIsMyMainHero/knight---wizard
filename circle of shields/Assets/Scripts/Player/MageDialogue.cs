using UnityEngine;

public class MageDialogue : MonoBehaviour
{
    [Header("Phrase Collections")]
    [SerializeField] private PhraseCollection introPhrases;
    [SerializeField] private PhraseCollection onParryPhrases;
    [SerializeField] private PhraseCollection onDamagePhrases;
    [SerializeField] private PhraseCollection onLowHPPhrases;
    [SerializeField] private PhraseCollection waveStartPhrases;
    [SerializeField] private PhraseCollection waveEndPhrases;
    [SerializeField] private PhraseCollection knightLowHPPhrases;
    [SerializeField] private PhraseCollection goldenAppearsPhrases;
    
    [Header("References")]
    [SerializeField] private Health knightHealth;
    [SerializeField] private Health mageHealth;
    
    [Header("Settings")]
    [SerializeField] private float parryPhraseChance = 0.3f;   // не каждый парри
    [SerializeField] private float lowHPThreshold = 0.3f;
    [SerializeField] private float lowHPCooldown = 15f;         // не спамить
    
    private float lastLowHPPhraseTime = -999f;
    private bool spokenLowHPWarning = false;
    
    private void Start()
    {
        if (mageHealth != null)
        {
            mageHealth.OnDamageTaken.AddListener(OnMageDamageTaken);
            mageHealth.OnHealed.AddListener(OnMageHealed);
        }
        
        if (knightHealth != null)
            knightHealth.OnDamageTaken.AddListener(OnKnightDamageTaken);
        
        Invoke(nameof(SpeakIntro), 1f);
    }
    
    private void SpeakIntro()
    {
        if (introPhrases != null)
            DialogueSystem.Instance?.ShowPhrase(introPhrases.GetRandomPhrase(), 4f, 1);
    }
    
    // Волшебница ранена
    private void OnMageDamageTaken(int damage)
    {
        float ratio = (float)mageHealth.CurrentHealth / mageHealth.MaxHealth;
        
        // Низкий HP предупреждение
        if (ratio < lowHPThreshold && !spokenLowHPWarning)
        {
            spokenLowHPWarning = true;
            if (onLowHPPhrases != null)
                DialogueSystem.Instance?.ShowPhrase(onLowHPPhrases.GetRandomPhrase(), 4f, 5);
        }
        
        // Обычный урон
        if (Random.value < 0.4f)
        {
            if (onDamagePhrases != null)
                DialogueSystem.Instance?.ShowPhrase(onDamagePhrases.GetRandomPhrase(), 2.5f, 3);
        }
    }
    
    private void OnMageHealed(int amount)
    {
        // Сброс флага если HP восстановилось
        float ratio = (float)mageHealth.CurrentHealth / mageHealth.MaxHealth;
        if (ratio > 0.5f)
            spokenLowHPWarning = false;
    }
    
    private void OnKnightDamageTaken(int damage)
    {
        float ratio = (float)knightHealth.CurrentHealth / knightHealth.MaxHealth;
        
        if (ratio < lowHPThreshold && Time.time - lastLowHPPhraseTime > lowHPCooldown)
        {
            lastLowHPPhraseTime = Time.time;
            if (knightLowHPPhrases != null)
                DialogueSystem.Instance?.ShowPhrase(knightLowHPPhrases.GetRandomPhrase(), 3f, 4);
        }
    }
    
    // Вызывается из ShieldHitbox при успешном парри
    public void OnParryHappened()
    {
        if (Random.value < parryPhraseChance)
        {
            if (onParryPhrases != null)
                DialogueSystem.Instance?.ShowPhrase(onParryPhrases.GetRandomPhrase(), 2f, 2);
        }
    }
    
    // Вызывается из WaveManager при старте волны
    public void OnWaveStart()
    {
        if (waveStartPhrases != null)
            DialogueSystem.Instance?.ShowPhrase(waveStartPhrases.GetRandomPhrase(), 3f, 3);
    }
    
    // Вызывается при завершении волны
    public void OnWaveEnd()
    {
        if (waveEndPhrases != null)
            DialogueSystem.Instance?.ShowPhrase(waveEndPhrases.GetRandomPhrase(), 3f, 2);
    }
    
    // Вызывается когда появляется золотой враг
    public void OnGoldenAppears()
    {
        if (goldenAppearsPhrases != null)
            DialogueSystem.Instance?.ShowPhrase(goldenAppearsPhrases.GetRandomPhrase(), 3f, 4);
    }
}