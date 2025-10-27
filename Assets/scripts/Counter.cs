 using UnityEngine;

public class Counter : MonoBehaviour
{
    private GameController gc;
    private GameFinisher gameFinisher;
    void Start()
    {
        gc = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameController>();
        gameFinisher = GetComponent<GameFinisher>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Ball"))
        {
            if (gc!=null)
            {
                Player currentPlayer = gc.GetPlayer();
                currentPlayer.IncrementBallsScored();
                currentPlayer.OnBallPocketed();
                Team scoredTeam = gc.GetTeamByType((TeamType)other.gameObject.layer);
                scoredTeam.IncrementScore();
                Debug.Log(scoredTeam.name + " " + scoredTeam.BallsOwnTypeScored);
                if(scoredTeam.IsScoredEveryOwnTypeBall())
                {
                    StartCoroutine(gameFinisher.FinishGame(scoredTeam));
                }
                gc.RemoveBall(other.gameObject);
            }
            else
            {
                Debug.LogError("Error , gamecontroller null");
            }
        }
    }
}
