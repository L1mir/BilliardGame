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

        UIController.Instance.SetWinningTeamText(winningTeam.GetTeamType());
        yield return new WaitForSeconds(timeAfterWinning);
    }
}
