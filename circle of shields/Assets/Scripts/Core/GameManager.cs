using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    
    [Header("References")]
    [SerializeField] private Health knightHealth;
    [SerializeField] private Health mageHealth;
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private GameObject victoryPanel;
    
    [Header("Settings")]
    [SerializeField] private float gameOverSlowdown = 0.3f;
    [SerializeField] private float restartDelay = 3f;
    [SerializeField] private float victoryRestartDelay = 10f;
    
    [Header("VFX")]
    [SerializeField] private GameObject parryParticlePrefab;
    
    private bool isGameOver = false;
    private bool isVictory = false;
    
    public bool IsGameOver => isGameOver;
    public bool IsVictory => isVictory;
    
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        
        // Регистрируем префаб для ParryEffect
        if (parryParticlePrefab != null)
            ParryEffect.ParticlePrefab = parryParticlePrefab;
    }
    
    private void Start()
    {
        Time.timeScale = 1f;
        
        if (knightHealth != null)
            knightHealth.OnDeath.AddListener(OnKnightDied);
        
        if (mageHealth != null)
            mageHealth.OnDeath.AddListener(OnMageDied);
        
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);
        
        if (victoryPanel != null)
            victoryPanel.SetActive(false);
    }
    
    private void OnKnightDied()
    {
        TriggerGameOver("The Knight has fallen!");
    }
    
    private void OnMageDied()
    {
        TriggerGameOver("The Mage has been slain!");
    }
    
    private void TriggerGameOver(string reason)
    {
        if (isGameOver || isVictory) return;
        
        isGameOver = true;
        Debug.Log("GAME OVER: " + reason);
        
        Time.timeScale = gameOverSlowdown;
        
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
            
            GameOverUI ui = gameOverPanel.GetComponent<GameOverUI>();
            if (ui != null)
                ui.SetReason(reason);
        }
        
        StartCoroutine(RestartAfterDelay(restartDelay));
    }
    
    public void TriggerVictory()
    {
        if (isGameOver || isVictory) return;
        
        isVictory = true;
        Debug.Log("=== VICTORY! ===");
        
        Time.timeScale = 0.5f;
        
        if (victoryPanel != null)
            victoryPanel.SetActive(true);
        
        StartCoroutine(RestartAfterDelay(victoryRestartDelay));
    }
    
    private IEnumerator RestartAfterDelay(float delay)
    {
        yield return new WaitForSecondsRealtime(delay);
        
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}