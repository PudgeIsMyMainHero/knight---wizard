using UnityEngine;

/// Базовый класс для всех эффектов апгрейдов.
/// Наследники применяют конкретные изменения к игре.
public abstract class UpgradeEffect : ScriptableObject
{
    [Header("Effect Info")]
    [SerializeField] protected string effectDescription = "Base effect";
    
    public string Description => effectDescription;
    
    /// Применить эффект к игре
    public abstract void Apply();
}