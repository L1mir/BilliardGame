using UnityEngine;

public abstract class GameModifier : MonoBehaviour
{
    [SerializeField] private string modifierName;
    [SerializeField, TextArea(2, 4)] private string description;
    [SerializeField] private float duration = 10f;
    [SerializeField] private Sprite icon;
    
    protected float remainingTime;
    private bool isActive;

    public string ModifierName => modifierName;
    
    public string Description => description;
    public Sprite Icon => icon;
    public float RemainingTime => remainingTime;
    public bool IsActive => isActive;

    public virtual void Activate()
    {
        if (isActive) return;
        isActive = true;
        remainingTime = duration;
        
        if (UIController.Instance != null)
        {
            UIController.Instance.ShowModifierNotification(modifierName, description);
        }
        
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