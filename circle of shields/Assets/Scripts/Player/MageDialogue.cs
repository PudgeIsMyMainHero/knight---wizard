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
        // Подписка на события
        if (mageHealth != null)
            mageHealth.OnHealthChanged.AddListener(OnMageHealthChanged);
        
        if (knightHealth != null)
            knightHealth.OnHealthChanged.AddListener(OnKnightHealthChanged);
        
        // Вводная фраза через 2 сек после старта
        Invoke(nameof(SpeakIntro), 2f);
    }
    
    private void SpeakIntro()
    {
        if (introPhrases != null)
            DialogueSystem.Instance?.ShowPhrase(introPhrases.GetRandomPhrase(), 4f, 1);
    }
    
    // Волшебница ранена
    private void OnMageHealthChanged(int current, int max)
    {
        float ratio = (float)current / max;
        
        // Низкий HP предупреждение
        if (ratio < lowHPThreshold && !spokenLowHPWarning)
        {
            spokenLowHPWarning = true;
            if (onLowHPPhrases != null)
                DialogueSystem.Instance?.ShowPhrase(onLowHPPhrases.GetRandomPhrase(), 4f, 5);   // высокий приоритет
        }
        
        // Восстановление флага при hp > 50%
        if (ratio > 0.5f)
            spokenLowHPWarning = false;
        
        // Обычный урон
        if (current < max && Random.value < 0.4f)
        {
            if (onDamagePhrases != null)
                DialogueSystem.Instance?.ShowPhrase(onDamagePhrases.GetRandomPhrase(), 2.5f, 3);
        }
    }
    
    // Рыцарь ранен
    private void OnKnightHealthChanged(int current, int max)
    {
        float ratio = (float)current / max;
        
        // Волшебница переживает за рыцаря
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