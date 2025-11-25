using UnityEngine;

namespace Abilities
{
    public abstract class Ability : MonoBehaviour
    {

        [SerializeField] protected int abilityCost = 0;
        
        public int AbilityCost => abilityCost;
        
        protected bool isActive = false;
        
        protected virtual bool Activate()
        {
            isActive = true;
            Debug.Log("Способность активирована!");
            OnActivate();
            return true;
        }
        
        protected abstract void OnActivate();
    }
}