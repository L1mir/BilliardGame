using UnityEngine;

namespace Abilities
{
    public abstract class Ability : MonoBehaviour
    {
        // Для теста оставляем 0. Потом стоимость можно менять у каждой способности отдельно в Inspector.
        [SerializeField] protected int abilityCost = 0;
        
        public int AbilityCost => abilityCost;
        
        protected bool isActive = false;
        
        protected virtual bool Activate()
        {
            isActive = true;
            if (!OnActivate())
            {
                isActive = false;
                return false;
            }

            return true;
        }

        protected bool TrySpendAbilityCost(Player player, GameController gameController)
        {
            if (player == null)
            {
                return false;
            }

            if (!player.SpendAbilityPoints(abilityCost))
            {
                UIController.Instance.ShowNotEnoughPointsNotification();
                return false;
            }

            gameController?.ShowCurrentPlayerInfo();
            return true;
        }

        protected abstract bool OnActivate();
    }
}
