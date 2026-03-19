using System.Collections.Generic;
using DefaultNamespace;
using UnityEngine;


public class TurnEffectManager : MonoBehaviour
{
    private readonly List<IEndOfTurnEffect> activeEffects = new();

    public void Register(IEndOfTurnEffect effect)
    {
        if (!activeEffects.Contains(effect))
            activeEffects.Add(effect);
    }

    public void Unregister(IEndOfTurnEffect effect)
    {
        activeEffects.Remove(effect);
    }

    public void EndTurn()
    {
        foreach (var effect in activeEffects.ToArray())
        {
            effect.OnTurnEnd();
        }
        activeEffects.Clear();
    }
}
