using DefaultNamespace;
using UnityEngine;

namespace Abilities
{
    public class GhostBallAbility : Ability, IEndOfTurnEffect
    {
        private Collider whiteBallCollider;
        private MaterialPropertyBlock mpb;

        private void Awake()
        {
            mpb = new MaterialPropertyBlock();
        }

        protected override bool OnActivate()
        {
            var allBalls = GameObject.FindGameObjectsWithTag("Ball");
            GameController gameController = FindObjectOfType<GameController>();
            if (gameController == null) return false;

            Player currentPlayer = gameController.GetPlayer();
            if (currentPlayer == null) return false;

            if (!TrySpendAbilityCost(currentPlayer, gameController))
            {
                return false;
            }

            Team currentTeam = currentPlayer.GetTeam();
            if (currentTeam == null) return false;

            int layerToIgnore = currentTeam.GetTeamType() == TeamType.Solid ? 
                LayerMask.NameToLayer("Strip") : 
                LayerMask.NameToLayer("Solid");

            var whiteBall = GameObject.FindGameObjectWithTag("WhiteBall");
            if (whiteBall == null) return false;

            whiteBallCollider = whiteBall.GetComponent<Collider>();
            if (whiteBallCollider == null) return false;

            var turnManager = FindObjectOfType<TurnEffectManager>();
            if (turnManager != null)
            {
                turnManager.Register(this);
            }

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
                        renderer.GetPropertyBlock(mpb);

                        Color color = renderer.sharedMaterial.color;
                        color.a = 0.4f;

                        mpb.SetColor("_BaseColor", color);

                        renderer.SetPropertyBlock(mpb);
                    }
                }   
            }

            return true;
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
                    renderer.GetPropertyBlock(mpb);

                    Color color = renderer.sharedMaterial.color;
                    color.a = 1f;

                    mpb.SetColor("_BaseColor", color);

                    renderer.SetPropertyBlock(mpb);
                }
            }

            isActive = false;
        }
    }
}
