using UnityEngine;

public abstract class GameModifier : MonoBehaviour
{
    [SerializeField] private string modifierName;
    [SerializeField] private float duration = 10f;
    [SerializeField] private Sprite icon;
    
    protected float remainingTime;
    private bool isActive;

    public string ModifierName => modifierName;
    public Sprite Icon => icon;
    public float RemainingTime => remainingTime;
    public bool IsActive => isActive;

    public virtual void Activate()
    {
        isActive = true;
        remainingTime = duration;
        OnActivate();
    }

    public virtual void Deactivate()
    {
        isActive = false;
        OnDeactivate();
    }

    protected virtual void Update()
    {
        if (!isActive) return;
        
        remainingTime -= Time.deltaTime;
        if (remainingTime <= 0)
        {
            Deactivate();
        }
    }

    protected abstract void OnActivate();
    protected abstract void OnDeactivate();
}