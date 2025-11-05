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
    private float stopThreshold = 0.5f;
    private float dampingCoefficient = 0.3f;
    private GameObject whiteBall;
    private Vector3 whiteBallStartPosition;

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
        currentPlayer++;
        
        if (currentPlayer >= teams[currentTeam].Size)
        {
            currentPlayer = 0;
            currentTeam = (currentTeam + 1) % TEAMS_AMOUNT;
        }
        
        var player = GetPlayer();
        player.isCurrentPlayer = true;
        
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
    
    private void ShowCurrentPlayerInfo()
    {
        string teamName = currentTeam == 0 ? "stripes" : "solids";
        UIController.Instance?.ShowCurrentPlayer(teamName);
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

    private void InitializeBalls()
    {
        var whiteBall = GameObject.FindGameObjectWithTag("WhiteBall");
        var whiteBallStartPos = GameObject.FindGameObjectWithTag("WhiteBallStartPosition");
        var ballPositions = GameObject.FindGameObjectsWithTag("StartPosition");
        var allBalls = GameObject.FindGameObjectsWithTag("Ball");
        
        balls = new List<GameObject>();
        
        if (whiteBall != null && whiteBallStartPos != null)
        {
            whiteBall.transform.position = whiteBallStartPos.transform.position;
            whiteBall.GetComponent<Rigidbody>().linearDamping = dampingCoefficient;
        }
        else
        {
            Debug.LogError("Белый шар или его стартовая позиция не найдены!");
            return;
        }

        var otherBalls = allBalls.Where(b => b != whiteBall).ToArray();

        var shuffledPositions = RandomSort(ballPositions);

        for (int i = 0; i < otherBalls.Length && i < shuffledPositions.Count; i++)
        {
            otherBalls[i].transform.position = shuffledPositions[i].transform.position;
            otherBalls[i].GetComponent<Rigidbody>().linearDamping = dampingCoefficient;
            balls.Add(otherBalls[i]);
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
        ShowCurrentPlayerInfo();
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
