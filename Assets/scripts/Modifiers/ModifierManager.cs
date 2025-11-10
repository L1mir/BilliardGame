using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class ModifierManager : MonoBehaviour
{
    public static ModifierManager Instance { get; private set; }
    
    [SerializeField] private List<GameModifier> availableModifiers = new List<GameModifier>();
    [SerializeField] private float minTimeBetweenModifiers = 30f;
    [SerializeField] private float maxTimeBetweenModifiers = 60f;
    
    private List<GameModifier> activeModifiers = new List<GameModifier>();
    private float timeUntilNextModifier;
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            
            // Инициализируем таймер для первого модификатора
            ResetModifierTimer();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    private void Update()
    {
        if (availableModifiers.Count == 0) return;
        
        timeUntilNextModifier -= Time.deltaTime;
        
        if (timeUntilNextModifier <= 0f)
        {
            ActivateRandomModifier();
            ResetModifierTimer();
        }

        for (int i = activeModifiers.Count - 1; i >= 0; i--)
        {
            if (!activeModifiers[i].IsActive)
            {
                activeModifiers.RemoveAt(i);
            }
        }
    }
    
    private void ResetModifierTimer()
    {
        timeUntilNextModifier = Random.Range(minTimeBetweenModifiers, maxTimeBetweenModifiers);
    }
    
    public void ActivateRandomModifier()
    {
        if (availableModifiers.Count == 0) return;
        
        int randomIndex = Random.Range(0, availableModifiers.Count);
        GameModifier modifierToActivate = availableModifiers[randomIndex];

        if (modifierToActivate != null)
        {
            GameModifier newModifier = Instantiate(modifierToActivate, transform);
            newModifier.Activate();
            activeModifiers.Add(newModifier);
        }
    }
    
    public void AddModifier(GameModifier modifier)
    {
        if (!availableModifiers.Contains(modifier))
        {
            availableModifiers.Add(modifier);
        }
    }
    
    public void RemoveModifier(GameModifier modifier)
    {
        if (availableModifiers.Contains(modifier))
        {
            availableModifiers.Remove(modifier);
        }
    }
    
    public void DeactivateAllModifiers()
    {
        foreach (var modifier in activeModifiers)
        {
            if (modifier != null)
            {
                modifier.Deactivate();
                Destroy(modifier.gameObject);
            }
        }
        
        activeModifiers.Clear();
    }
}