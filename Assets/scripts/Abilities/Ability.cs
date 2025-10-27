using UnityEngine;

namespace Abilities
{
    public abstract class Ability : MonoBehaviour
    {
        [Tooltip("Сколько очков нужно для активации способности")]
        [SerializeField] protected int pointsRequired = 1;
        
        protected int currentPoints = 0;
        protected bool isActive = false;
        
        public bool IsReady => currentPoints >= pointsRequired;
        
        public float Progress => Mathf.Clamp01((float)currentPoints / pointsRequired);

        public void AddPoint()
        {
            if (IsReady) return;
            
            currentPoints++;
            Debug.Log($"Очков способности: {currentPoints}/{pointsRequired}");
        }
        
        public virtual bool Activate()
        {
            //if (!IsReady || isActive) return false;
            
            isActive = true;
            currentPoints = 0;
            Debug.Log("Способность активирована!");
            OnActivate();
            return true;
        }
        
        protected abstract void OnActivate();
    }
}