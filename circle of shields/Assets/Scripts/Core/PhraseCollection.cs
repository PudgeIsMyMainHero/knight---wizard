using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Phrases_", menuName = "CircleOfShields/Phrase Collection")]
public class PhraseCollection : ScriptableObject
{
    public string collectionName = "General";
    public List<string> phrases = new List<string>();
    
    public string GetRandomPhrase()
    {
        if (phrases == null || phrases.Count == 0) return null;
        return phrases[Random.Range(0, phrases.Count)];
    }
}