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
            OnActivate();
            return true;
        }
        
        protected abstract void OnActivate();
    }
}