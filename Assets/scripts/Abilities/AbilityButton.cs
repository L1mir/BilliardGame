using UnityEngine;
using UnityEngine.UI;

namespace Abilities
{
    public class AbilityButton : MonoBehaviour
    {
        private Player currentPlayer;
        private Button button;

        private void Start()
        {
            button = GetComponent<Button>();
            button.onClick.AddListener(OnButtonClick);
        }

        private void OnButtonClick()
        {
            // Получаем текущего игрока из GameController
            var gc = FindObjectOfType<GameController>();
            if (gc != null)
            {
                currentPlayer = gc.GetPlayer();
                if (currentPlayer != null)
                {
                    currentPlayer.UseAbility();
                    return;
                }
            }
            Debug.LogError("Player is not assigned to AbilityButton!");
        }
    }
}