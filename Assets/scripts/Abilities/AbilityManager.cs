using System.Collections.Generic;
using Abilities;
using UnityEngine;

public class AbilityManager : MonoBehaviour
{
    public static AbilityManager Instance { get; private set; }
    
    [SerializeField] private Ability[] allAbilities;
    
    private Dictionary<string, Ability> abilitiesMap = new();
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            foreach (var ability in allAbilities)
            {
                if (ability != null)
                {
                    abilitiesMap[ability.GetType().Name] = ability;
                }
            }
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void UpdateAbilityProgress(Player player, float progress)
    {
        foreach (var ability in allAbilities)
        {
            //ability.UpdateProgress(progress);
        }
    }
}