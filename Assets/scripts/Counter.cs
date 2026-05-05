using UnityEngine;

public class Counter : MonoBehaviour
{
    private GameController gc;
    private GameFinisher gameFinisher;

    private void Start()
    {
        gc = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameController>();
        gameFinisher = GetComponent<GameFinisher>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (gc == null)
        {
            Debug.LogError("GameController is null in Counter!");
            return;
        }

        if (other.CompareTag("BlackBall"))
        {
            Player currentPlayer = gc.GetPlayer();
            Team currentTeam = currentPlayer.GetTeam();
            Team otherTeam = gc.GetOtherTeam(currentTeam);
            if (currentTeam.IsScoredEveryOwnTypeBall())
            {
                Debug.Log("WIN: black ball last");
                StartCoroutine(gameFinisher.FinishGame(currentTeam));
            }
            else
            {
                Debug.Log("LOSE: black ball too early");
                StartCoroutine(gameFinisher.FinishGame(otherTeam));
            }

            gc.RemoveBall(other.gameObject);
            return;
        }

        if (other.CompareTag("Ball"))
        {
            Player currentPlayer = gc.GetPlayer();
            Team currentTeam = currentPlayer.GetTeam();
            TeamType ballType = (TeamType)other.gameObject.layer;
            if (!gc.AreTeamTypesAssigned)
            {
                gc.AssignTeamTypes(currentTeam, ballType);
            }

            Team scoringTeam = gc.GetTeamByType(ballType);
            if (scoringTeam != null)
            {
                scoringTeam.IncrementScore();
            }

            currentPlayer.IncrementBallsScored();
            currentPlayer.AddAbilityPoints(1);
            gc.ShowCurrentPlayerInfo();

            gc.RemoveBall(other.gameObject);
            if (ModifierManager.Instance != null)
            {
                gc.QueueModifierActivation();
            }
        }
        else if (other.CompareTag("WhiteBall"))
        {
            GameObject whiteBall = other.gameObject;
            GameObject whiteBallStartPos = GameObject.FindGameObjectWithTag("WhiteBallStartPosition");
            if (whiteBall != null && whiteBallStartPos != null)
            {
                Rigidbody rb = whiteBall.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.linearVelocity = Vector3.zero;
                    rb.angularVelocity = Vector3.zero;
                    rb.MovePosition(whiteBallStartPos.transform.position);
                    whiteBall.GetComponent<BallPhysics>()?.RecalibrateSurfaceHeight();
                }
                else
                {
                    whiteBall.transform.position = whiteBallStartPos.transform.position;
                    whiteBall.GetComponent<BallPhysics>()?.RecalibrateSurfaceHeight();
                }
            }
        }
    }
}
