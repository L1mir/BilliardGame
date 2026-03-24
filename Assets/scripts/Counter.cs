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
        if (other.CompareTag("Ball"))
        {
            if (gc != null)
            {
                Player currentPlayer = gc.GetPlayer();
                Team currentTeam = currentPlayer.GetTeam();
                Team otherTeam = gc.GetOtherTeam(currentTeam);

                TeamType ballType = (TeamType)other.gameObject.layer;

                if (!gc.AreTeamTypesAssigned)
                {
                    gc.AssignTeamTypes(currentTeam, ballType);
                }

                TeamType currentTeamType = currentTeam.GetTeamType();

                if (ballType == currentTeamType)
                {
                    currentPlayer.AbilityPoints += 1;
                    Debug.Log("Current player points:" + currentPlayer.AbilityPoints);

                    currentPlayer.IncrementBallsScored();
                    currentTeam.IncrementScore();

                    Debug.Log(currentTeam.name + " " + currentTeam.BallsOwnTypeScored);

                    if (currentTeam.IsScoredEveryOwnTypeBall())
                    {
                        StartCoroutine(gameFinisher.FinishGame(currentTeam));
                    }
                }
                else
                {
                    otherTeam.IncrementScore();
                }
                
                gc.RemoveBall(other.gameObject);

                if (ModifierManager.Instance != null)
                {
                    gc.QueueModifierActivation();
                    Debug.Log("Random Modifier activated");
                }
            }
            else
            {
                Debug.LogError("Error , gamecontroller null");
            }
        }
        else if (other.CompareTag("WhiteBall"))
        {
            var whiteBall = other.gameObject;
            var whiteBallStartPos = GameObject.FindGameObjectWithTag("WhiteBallStartPosition");
            if (whiteBall != null && whiteBallStartPos != null)
            {
                Rigidbody rb = whiteBall.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.linearVelocity = Vector3.zero;
                    rb.angularVelocity = Vector3.zero;
                    rb.MovePosition(whiteBallStartPos.transform.position);
                }
                else
                {
                    whiteBall.transform.position = whiteBallStartPos.transform.position;
                }
            }
        }
    }
}
