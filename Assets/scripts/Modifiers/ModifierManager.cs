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
            return;
        }

        int inactiveCount = 0;
        foreach (var modifier in availableModifiers)
        {
            if (modifier != null && !modifier.IsActive)
            {
                inactiveCount++;
            }
        }

        if (inactiveCount == 0)
        {
            return;
        }

        int targetIndex = Random.Range(0, inactiveCount);
        foreach (var modifier in availableModifiers)
        {
            if (modifier == null || modifier.IsActive)
            {
                continue;
            }

            if (targetIndex == 0)
            {
                modifier.Activate();
                return;
            }

            targetIndex--;
        }
    }
}
