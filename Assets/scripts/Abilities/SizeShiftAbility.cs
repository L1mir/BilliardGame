using System.Collections.Generic;
using UnityEngine;

namespace Abilities
{
    public class SizeShiftAbility : Ability
    {
        [SerializeField] private float shrinkFactor = 0.5f;
        [SerializeField] private float duration = 10f;
        
        private Dictionary<GameObject, (Vector3 scale, float drag, float angularDrag)> originalValues = new();
        private GameObject currentPlayerBall;
        
        protected override void OnActivate()
        {
            var gameController = FindObjectOfType<GameController>();
            if (gameController == null) return;
            
            var allBalls = GameObject.FindGameObjectsWithTag("Ball");
            
            foreach (var ball in allBalls)
            {
                if (ball.activeInHierarchy && ball.TryGetComponent<Rigidbody>(out var rb))
                {
                    originalValues[ball] = (
                        ball.transform.localScale,
                        rb.linearDamping,
                        rb.angularDamping
                    );
                    
                    ball.transform.localScale *= shrinkFactor;
                    rb.linearDamping *= 0.5f;
                    rb.angularDamping *= 0.5f;

                    if (ball.TryGetComponent<SphereCollider>(out var collider))
                    {
                        collider.radius = 0.5f;
                    }
                }
            }

            Invoke(nameof(ResetBalls), duration);
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
                if (ball != null && ball.activeInHierarchy && ball.TryGetComponent<Rigidbody>(out var rb))
                {
                    ball.transform.localScale = pair.Value.scale;
                    rb.linearDamping = pair.Value.drag;
                    rb.angularDamping = pair.Value.angularDrag;

                    if (ball.TryGetComponent<SphereCollider>(out var collider))
                    {
                        collider.radius = 0.5f;
                    }
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