using UnityEngine;

namespace Abilities
{
    public class BallSwapAbility : Ability
    {
        [Tooltip("Ссылка на белый шар")]
        [SerializeField] private GameObject whiteBall;
    
        // Теги для определения шаров команд
        private const string SOLID_LAYER = "Solid";
        private const string STRIP_LAYER = "Strip";
        private const string WHITE_BALL_TAG = "WhiteBall";

        public void UseBallSwap()
        {
            OnActivate();
        }
        
        protected override void OnActivate()
        {
            if (isActive) return;
            isActive = true;

            GameController gameController = FindObjectOfType<GameController>();
            if (gameController == null)
            {
                Debug.LogError("GameController not found!");
                return;
            }

            Player currentPlayer = gameController.GetPlayer();
            if (currentPlayer == null)
            {
                Debug.LogError("Current player not found!");
                return;
            }

            Team currentTeam = currentPlayer.GetTeam();
            if (currentTeam == null)
            {
                Debug.LogError("Current team not found!");
                return;
            }

            if (whiteBall == null)
            {
                whiteBall = GameObject.FindGameObjectWithTag(WHITE_BALL_TAG);
                if (whiteBall == null)
                {
                    Debug.LogError("White ball not found! Make sure it has the 'WhiteBall' tag.");
                    return;
                }
            }
        
            string targetLayer = currentTeam.GetTeamType() == TeamType.Solid ? SOLID_LAYER : STRIP_LAYER;
            int targetLayerMask = LayerMask.NameToLayer(targetLayer);

            var allBalls = GameObject.FindGameObjectsWithTag("Ball");
            var teamBalls = System.Array.FindAll(allBalls, ball => 
                ball.layer == targetLayerMask && ball != whiteBall);

            if (teamBalls == null || teamBalls.Length == 0)
            {
                Debug.LogWarning("No team balls found!");
                return;
            }

            GameObject randomBall = teamBalls[Random.Range(0, teamBalls.Length)];

            if (randomBall != null)
            {
                Vector3 whiteBallPosition = whiteBall.transform.position;
                whiteBall.transform.position = randomBall.transform.position;
                randomBall.transform.position = whiteBallPosition;
                
                Debug.Log($"Switched positions between white ball and {randomBall.name}");
            }

            isActive = false;
        }
    }
}