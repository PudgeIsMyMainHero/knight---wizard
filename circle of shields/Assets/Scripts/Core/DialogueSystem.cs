using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DialogueSystem : MonoBehaviour
{
    public static DialogueSystem Instance { get; private set; }
    
    [Header("Speech Bubble")]
    [SerializeField] private GameObject speechBubble;
    [SerializeField] private TextMeshProUGUI bubbleText;
    [SerializeField] private CanvasGroup bubbleCanvasGroup;
    
    [Header("Follow Target")]
    [SerializeField] private Transform speakerTransform;   // Волшебница
    [SerializeField] private Vector3 bubbleOffset = new Vector3(0, 2, 0);
    
    [Header("Timing")]
    [SerializeField] private float defaultDisplayTime = 3f;
    [SerializeField] private float fadeInTime = 0.2f;
    [SerializeField] private float fadeOutTime = 0.3f;
    [SerializeField] private float globalCooldown = 1f;   // минимум между фразами
    
    private float lastPhraseTime = -999f;
    private Coroutine currentPhraseCoroutine;
    private int currentPriority = 0;
    
    private Camera mainCamera;

    private bool isBlocked = false;
    
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        
        mainCamera = Camera.main;
    }
    
    private void Start()
    {
        if (speechBubble != null)
            speechBubble.SetActive(false);
    }
    
    private void LateUpdate()
    {
        // Bubble следует за волшебницей (world space → screen space)
        if (speechBubble != null && speechBubble.activeSelf && speakerTransform != null && mainCamera != null)
        {
            Vector3 worldPos = speakerTransform.position + bubbleOffset;
            Vector3 screenPos = mainCamera.WorldToScreenPoint(worldPos);
            speechBubble.transform.position = screenPos;
        }
    }
    
    /// Показать фразу
    /// priority: 0 (низкий) - 10 (высокий). Более высокий приоритет прерывает низкий.
    public void ShowPhrase(string phrase, float duration = -1f, int priority = 1)
    {
        // Проверка cooldown
        if (Time.time - lastPhraseTime < globalCooldown && priority <= currentPriority)
            return;
        if (isBlocked) return;
        
        // Прерываем текущую если новая важнее
        if (currentPhraseCoroutine != null)
        {
            if (priority < currentPriority) return;   // менее важная — игнор
            StopCoroutine(currentPhraseCoroutine);
        }
        
        if (duration < 0) duration = defaultDisplayTime;
        
        currentPriority = priority;
        currentPhraseCoroutine = StartCoroutine(DisplayPhrase(phrase, duration));
    }
    
    private IEnumerator DisplayPhrase(string phrase, float duration)
    {
        lastPhraseTime = Time.time;
        
        if (bubbleText != null)
            bubbleText.text = phrase;
        
        if (speechBubble != null)
            speechBubble.SetActive(true);
        
        // Fade In
        if (bubbleCanvasGroup != null)
        {
            float t = 0;
            while (t < fadeInTime)
            {
                t += Time.deltaTime;
                bubbleCanvasGroup.alpha = t / fadeInTime;
                yield return null;
            }
            bubbleCanvasGroup.alpha = 1;
        }
        
        // Показываем
        yield return new WaitForSeconds(duration);
        
        // Fade Out
        if (bubbleCanvasGroup != null)
        {
            float t = 0;
            while (t < fadeOutTime)
            {
                t += Time.deltaTime;
                bubbleCanvasGroup.alpha = 1 - (t / fadeOutTime);
                yield return null;
            }
            bubbleCanvasGroup.alpha = 0;
        }
        
        if (speechBubble != null)
            speechBubble.SetActive(false);
        
        currentPhraseCoroutine = null;
        currentPriority = 0;
    }
    
    public void BlockDialogue(bool block)
    {
        isBlocked = block;
        
        // Если блокируем — скрываем текущую фразу
        if (block)
        {
            if (currentPhraseCoroutine != null)
            {
                StopCoroutine(currentPhraseCoroutine);
                currentPhraseCoroutine = null;
            }
            
            if (speechBubble != null)
                speechBubble.SetActive(false);
            
            currentPriority = 0;
        }
    }
    
    /// Показать случайную фразу из списка
    public void ShowRandomPhrase(List<string> phrases, float duration = -1f, int priority = 1)
    {
        if (phrases == null || phrases.Count == 0) return;
        
        string phrase = phrases[Random.Range(0, phrases.Count)];
        ShowPhrase(phrase, duration, priority);
    }
}