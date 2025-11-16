using UnityEngine;

public class ModifierManager : MonoBehaviour
{
    public static ModifierManager Instance { get; private set; }

    [SerializeField] private GameModifier[] availableModifiers;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void ActivateRandomModifier()
    {
        if (availableModifiers == null || availableModifiers.Length == 0)
        {
            Debug.LogError("No modifiers assigned in ModifierManager!");
            return;
        }

        int randomIndex = Random.Range(0, availableModifiers.Length);
        GameModifier modifier = availableModifiers[randomIndex];

        if (modifier != null && !modifier.IsActive)
        {
            modifier.Activate();
            Debug.Log($"Activated modifier: {modifier.ModifierName}");
        }
    }
}