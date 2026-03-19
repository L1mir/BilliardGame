using DefaultNamespace;
using UnityEngine;

namespace Abilities
{
    public class GhostBallAbility : Ability, IEndOfTurnEffect
    {
        private Collider whiteBallCollider;

        private void Awake()
        {
            abilityCost = 0;
        }

        protected override void OnActivate()
        {
            var turnManager = FindObjectOfType<TurnEffectManager>();
            if (turnManager != null)
            {
                turnManager.Register(this);
            }
            
            var allBalls = GameObject.FindGameObjectsWithTag("Ball");
            GameController gameController = FindObjectOfType<GameController>();
            if (gameController == null) return;

            Player currentPlayer = gameController.GetPlayer();
            if (currentPlayer == null) return;

            if (currentPlayer.AbilityPoints < abilityCost)
            {
                Debug.Log("Ghost Ball points declined");
                return;
            }

            currentPlayer.AbilityPoints -= abilityCost;
            gameController.ShowCurrentPlayerInfo();

            Team currentTeam = currentPlayer.GetTeam();
            if (currentTeam == null) return;

            int layerToIgnore = currentTeam.GetTeamType() == TeamType.Solid ? 
                LayerMask.NameToLayer("Strip") : 
                LayerMask.NameToLayer("Solid");

            var whiteBall = GameObject.FindGameObjectWithTag("WhiteBall");
            if (whiteBall == null) return;

            whiteBallCollider = whiteBall.GetComponent<Collider>();
            if (whiteBallCollider == null) return;

            foreach (var ball in allBalls)
            {
                if (ball.CompareTag("WhiteBall")) continue;

                var ballCollider = ball.GetComponent<Collider>();
                if (ballCollider == null) continue;

                if (ball.layer == layerToIgnore)
                {
                    Physics.IgnoreCollision(whiteBallCollider, ballCollider, true);
                    
                    var renderer = ball.GetComponent<Renderer>();
                    if (renderer != null)
                    {
                        Color color = renderer.material.color;
                        color.a = 0.5f;
                        renderer.material.color = color;
                    }
                }
            }
        }
        
        public void OnTurnEnd()
        {
            Deactivate();

            var turnManager = FindObjectOfType<TurnEffectManager>();
            if (turnManager != null)
            {
                turnManager.Unregister(this);
            }
        }
        
        public void ActivateGhostBall()
        {
            if (isActive) return;
            Debug.Log("ActivateGhostBall вызван");
            Activate();
        }

        private void Deactivate()
        {
            if (!isActive) return;

            var allBalls = GameObject.FindGameObjectsWithTag("Ball");
            if (whiteBallCollider == null) return;

            foreach (var ball in allBalls)
            {
                if (ball.CompareTag("WhiteBall")) continue;

                var ballCollider = ball.GetComponent<Collider>();
                if (ballCollider == null) continue;

                Physics.IgnoreCollision(whiteBallCollider, ballCollider, false);
                
                var renderer = ball.GetComponent<Renderer>();
                if (renderer != null)
                {
                    Color color = renderer.material.color;
                    color.a = 1f;
                    renderer.material.color = color;
                }
            }

            isActive = false;
        }
    }
}