using UnityEngine;
using System.Collections.Generic;

public class MageThreatMonitor : MonoBehaviour
{
    public static MageThreatMonitor Instance { get; private set; }
    
    [Header("Threat Limits")]
    [SerializeField] private int maxIncomingProjectiles = 3;
    
    private List<Projectile> incomingProjectiles = new List<Projectile>();
    
    public bool CanShootMage => incomingProjectiles.Count < maxIncomingProjectiles;
    
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }
    
    public void RegisterIncoming(Projectile proj)
    {
        if (!incomingProjectiles.Contains(proj))
            incomingProjectiles.Add(proj);
    }
    
    public void UnregisterIncoming(Projectile proj)
    {
        incomingProjectiles.Remove(proj);
    }
    
    private void Update()
    {
        // Чистим уничтоженные
        incomingProjectiles.RemoveAll(p => p == null);
    }
}