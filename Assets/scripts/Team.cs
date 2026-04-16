using UnityEngine;

public class Team : MonoBehaviour
{
    private const int BALLS_TO_WIN = 7;
    [SerializeField] private TeamType teamType;
    public int BallsOwnTypeScored { get; private set; } = 0;
    [SerializeField] private Player[] players;
    
    public void SetTeamType(TeamType teamType)
    {
        this.teamType = teamType;
    }
    
    public int Size
    {
        get
        {
            return players.Length;
        }
    }

    public Player GetPlayer(int index)
    {
        if (index >= players.Length) return null;
        return players[index];
    }
    public void IncrementScore()
    {
        BallsOwnTypeScored++;
    }
    
    public bool IsScoredEveryOwnTypeBall()
    {
        return BallsOwnTypeScored >= BALLS_TO_WIN;
    }
    
    public TeamType GetTeamType()
    {
        return teamType;
    }
}
