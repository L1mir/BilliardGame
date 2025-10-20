using System.Collections;
using UnityEngine;

public class GameFinisher : MonoBehaviour
{
    [SerializeField] private float timeAfterWinning = 5f;
    public IEnumerator FinishGame(Team winningTeam)
    {
        UIController.Instance.SetWinningTeamText(winningTeam.GetTeamType());
        yield return new WaitForSeconds(timeAfterWinning);
    }
}
