using UnityEngine;
using Abilities;

public class AbilityHotkeyHandler : MonoBehaviour
{
    [Header("Привязка способностей к клавишам")]
    [SerializeField] private SizeShiftAbility sizeShiftAbility;
    [SerializeField] private BallSwapAbility ballSwapAbility;
    [SerializeField] private GhostBallAbility ghostBallAbility;

    
    [Header("Настройки")]
    [SerializeField] private KeyCode ability1Key = KeyCode.Alpha1;
    [SerializeField] private KeyCode ability2Key = KeyCode.Alpha2;
    [SerializeField] private KeyCode ability3Key = KeyCode.Alpha3;
    
    [SerializeField] private bool showDebugLogs = true;
    
    private GameController gameController;
    private Player currentPlayer;
    
    private void Start()
    {
        gameController = FindObjectOfType<GameController>();
        
        if (sizeShiftAbility == null)
            sizeShiftAbility = FindObjectOfType<SizeShiftAbility>();
        if (ghostBallAbility == null)
            ghostBallAbility = FindObjectOfType<GhostBallAbility>();
        if (ballSwapAbility == null)
            ballSwapAbility = FindObjectOfType<BallSwapAbility>();
    }
    
    private void Update()
    {
        if (gameController == null) return;
        
        currentPlayer = gameController.GetPlayer();
        if (currentPlayer == null) return;
        
        if (!currentPlayer.isCurrentPlayer) return;
        
        if (Input.GetKeyDown(ability1Key))
        {
            if (sizeShiftAbility != null)
            {
                DebugLog("Активация Size Shift (клавиша 1)");
                sizeShiftAbility.ActivateSizeShiftAbility();
            }
        }
        
        if (Input.GetKeyDown(ability2Key))
        {
            if (ballSwapAbility != null)
            {
                DebugLog("Активация Ball Swap (клавиша 3)");
                ballSwapAbility.UseBallSwap();
            }
        }
        
        if (Input.GetKeyDown(ability3Key))
        {
            if (ghostBallAbility != null)
            {
                DebugLog("Активация Ghost Ball (клавиша 2)");
                ghostBallAbility.ActivateGhostBall();
            }
        }
    }
    
    private void DebugLog(string message)
    {
        if (showDebugLogs)
            Debug.Log($"[AbilityHotkey] {message}");
    }
}