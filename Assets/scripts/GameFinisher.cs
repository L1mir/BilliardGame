using System.Collections;
using UnityEngine;

public class GameFinisher : MonoBehaviour
{
    [SerializeField] private float timeAfterWinning = 5f;
    private bool isGameFinished = false;

    public IEnumerator FinishGame(Team winningTeam)
    {
        if (isGameFinished) yield break;

        isGameFinished = true;

        Time.timeScale = 0f;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        
        UIController.Instance?.ShowAll();

        UIController.Instance.SetWinningTeamText(winningTeam.GetTeamType());
    }
}
