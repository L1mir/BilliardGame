using System.Collections.Generic;
using DefaultNamespace;
using UnityEngine;

namespace Abilities
{
    public class SizeShiftAbility : Ability, IEndOfTurnEffect
    {
        [SerializeField] private float shrinkFactor = 0.5f;
        [SerializeField] private float duration = 10f;
        
        private Dictionary<GameObject, (Vector3 scale, float drag, float angularDrag, float radius)> originalValues = new();
        private GameObject currentPlayerBall;
        
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
            
            var gameController = FindObjectOfType<GameController>();
            if (gameController == null) return;
            
            Player currentPlayer = gameController.GetPlayer();
            if (currentPlayer.AbilityPoints < abilityCost)
            {
                Debug.Log("No points!");
                return;
            }
            
            currentPlayer.AbilityPoints -= abilityCost;
            gameController.ShowCurrentPlayerInfo();
            
            var allBalls = GameObject.FindGameObjectsWithTag("Ball");
            
            foreach (var ball in allBalls)
            {
                if (ball.activeInHierarchy && 
                    ball.TryGetComponent<Rigidbody>(out var rb) && 
                    ball.TryGetComponent<SphereCollider>(out var collider))
                {
                    originalValues[ball] = (
                        ball.transform.localScale,
                        rb.linearDamping,
                        rb.angularDamping,
                        collider.radius
                    );
                    
                    ball.transform.localScale *= shrinkFactor;
                    rb.linearDamping *= 0.5f;
                    rb.angularDamping *= 0.5f;

                    collider.radius *= shrinkFactor;
                }
            }
        }
        
        public void OnTurnEnd()
        {
            ResetBalls();

            var turnManager = FindObjectOfType<TurnEffectManager>();
            if (turnManager != null)
            {
                turnManager.Unregister(this);
            }
        }
        
        public void ActivateSizeShiftAbility()
        {
            if (isActive) return;
            Activate();
        }
        
        private void ResetBalls()
        {
            if (!isActive) return;
            
            foreach (var pair in originalValues)
            {
                var ball = pair.Key;
                if (ball != null && 
                    ball.activeInHierarchy && 
                    ball.TryGetComponent<Rigidbody>(out var rb) &&
                    ball.TryGetComponent<SphereCollider>(out var collider))
                {
                    ball.transform.localScale = pair.Value.scale;
                    rb.linearDamping = pair.Value.drag;
                    rb.angularDamping = pair.Value.angularDrag;
                    collider.radius = pair.Value.radius;
                }
            }
            
            originalValues.Clear();
            isActive = false;
            Debug.Log("Способность SizeShift сброшена - размеры шаров восстановлены");
        }
        
        private void OnDisable()
        {
            if (isActive)
            {
                ResetBalls();
            }
        }
    }
}