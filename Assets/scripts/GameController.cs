using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameController : MonoBehaviour
{
    private const int TEAMS_AMOUNT = 2;

    [SerializeField] private int teamSize = 1;
    [SerializeField] private Team[] teams = new Team[TEAMS_AMOUNT];

    private bool isMoveInProgress = false;
    
    private GameObject[] initialBalls;
    private GameObject[] startPositions;
    private List<GameObject> balls;
    private int currentPlayer = 0;
    private int currentTeam = 0;
    private float stopThreshold = 0.01f;
    private float dampingCoefficient = 0.5f;

    private void Awake()
    {
        InitializeBalls();
        GetPlayer().isCurrentPlayer = true;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.E) && !isMoveInProgress)
        {
            isMoveInProgress = true;
            StartCoroutine(GetPlayer().MakeStrike());
        }

        if (Input.GetKeyDown(KeyCode.R) && isMoveInProgress)
        {
            ForceNextMove();
        }
    }

    public void ForceNextMove()
    {
        foreach (var ball in balls)
        {
            if (ball.activeInHierarchy)
            {
                Rigidbody rb = ball.GetComponent<Rigidbody>();
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }
        }
        
        NextMove();
        isMoveInProgress = false;
    }

    public Player GetPlayer(int index = -1,int team = -1)
    {
        if (index == -1 && team == -1) return teams[currentTeam].GetPlayer(currentPlayer);
        if (team >= 2) return null; 
        if (index >= teams[currentTeam].Size) return null;
        if(team == -1)
        {
            return teams[currentTeam].GetPlayer(index);
        }
        if (index == -1)
        {
            return teams[team].GetPlayer(currentPlayer);
        }
        return teams[team].GetPlayer(index);
    }

    public void InitializeBalls()
    {
        initialBalls = GameObject.FindGameObjectsWithTag("Ball");
        foreach (var ball in initialBalls) ball.GetComponent<Rigidbody>().linearDamping = dampingCoefficient;
        balls = RandomSort(initialBalls);
        startPositions = GameObject.FindGameObjectsWithTag("StartPosition");
        for (int i = 0;i < balls.Count;i++)
        {
            balls[i].transform.position = startPositions[i].transform.position;
        }
    }

    private List<T> RandomSort<T>(T[] collection)
    {
        System.Random random = new System.Random();
        HashSet<int> usedObjects = new HashSet<int>();
        List<T> sortedList = new List<T>();
        for(int index = 0;index < collection.Length;index++)
        {
           int rand = random.Next(0, collection.Length);
           while (usedObjects.Contains(rand))
           {
                rand = random.Next(0, collection.Length);
           }
           usedObjects.Add(rand);
           sortedList.Add(collection[rand]);
        }
        return sortedList;
    }

    public bool IsReadyToMove()
    {
        if (!isMoveInProgress) return true;

        foreach (var ball in balls)
        {
            if (ball.activeInHierarchy &&
                (Math.Abs(ball.GetComponent<Rigidbody>().linearVelocity.x) > stopThreshold ||
                 Math.Abs(ball.GetComponent<Rigidbody>().linearVelocity.y) > stopThreshold ||
                 Math.Abs(ball.GetComponent<Rigidbody>().linearVelocity.z) > stopThreshold))
            {
                return false;
            }
        }
        
        isMoveInProgress = false;
        return true;
    }

    public void NextMove()
    {
        var current = GetPlayer();
        current.isCurrentPlayer = false;
    
        if (currentTeam == 1 && currentPlayer + 1 == teamSize)
        {
            currentTeam = 0;
            currentPlayer = 0;
        }
        else if (currentPlayer + 1 == teamSize)
        {
            currentPlayer = 0;
            currentTeam++;
        }
        else
        {
            currentPlayer++;
        }
    
        GetPlayer().isCurrentPlayer = true;
    }

    public void NextBall(int axis,ref int currentBall)
    {
        currentBall += axis;
        if (currentBall >= balls.Count) currentBall = 0;
        if (currentBall < 0) currentBall = balls.Count - 1;
    }

    public GameObject GetBall(int index)
    {
        if (index >= balls.Count)
        {

            GetPlayer().CurrentBall = balls.Count-1;
            return balls[GetPlayer().CurrentBall];
        }
        return balls[index];
    }

    public void RemoveBall(GameObject ball)
    {
        ball.SetActive(false);
        balls.Remove(ball);
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Ball"))
        {
            ReturnBallOnBoard(other.gameObject);
        }
    }

    private void ReturnBallOnBoard(GameObject ball)
    {
        ball.GetComponent<Rigidbody>().angularVelocity = new Vector3(0, 0, 0);
        ball.GetComponent<Rigidbody>().linearVelocity = new Vector3(0, 0, 0);
        ball.transform.position = GetPlayer().previousBallPosition;
    }

    public Team GetTeamByType(TeamType type)
    {
        foreach(Team team in teams)
        {
            if(team.GetTeamType() == type)
            {
                return team;
            }
        }
        return null;
    }
}
