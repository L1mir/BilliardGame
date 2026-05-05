using System.Collections.Generic;
using DefaultNamespace;
using UnityEngine;

namespace Abilities
{
    public class SizeShiftAbility : Ability, IEndOfTurnEffect
    {
        [SerializeField] private float shrinkFactor = 0.5f;

        private Dictionary<GameObject, (Vector3 scale, float mass, float drag, float angularDrag)> originalValues = new();

        protected override bool OnActivate()
        {
            GameController gameController = FindObjectOfType<GameController>();
            if (gameController == null)
            {
                return false;
            }

            Player currentPlayer = gameController.GetPlayer();
            if (!TrySpendAbilityCost(currentPlayer, gameController))
            {
                return false;
            }

            var turnManager = FindObjectOfType<TurnEffectManager>();
            if (turnManager != null)
            {
                turnManager.Register(this);
            }

            var allBalls = GameObject.FindGameObjectsWithTag("Ball");

            foreach (var ball in allBalls)
            {
                if (!ball.activeInHierarchy) continue;

                if (ball.TryGetComponent<Rigidbody>(out var rb) &&
                    ball.TryGetComponent<SphereCollider>(out var collider))
                {
                    originalValues[ball] = (
                        ball.transform.localScale,
                        rb.mass,
                        rb.linearDamping,
                        rb.angularDamping
                    );

                    ApplyScaleKeepingBallOnTable(ball, collider, shrinkFactor);

                    rb.mass *= shrinkFactor;          // легче → быстрее
                    rb.linearDamping *= 0.9f;         // чуть быстрее катится
                    rb.angularDamping *= 0.9f;

                    var physics = ball.GetComponent<BallPhysics>();
                    physics?.SyncWithCurrentTransform();
                }
            }

            return true;
        }

        public void ActivateSizeShiftAbility()
        {
            if (isActive) return;

            Activate();
        }

        public void OnTurnEnd()
        {
            ResetBalls();

            var turnManager = FindObjectOfType<TurnEffectManager>();
            if (turnManager != null)
                turnManager.Unregister(this);
        }

        private void ResetBalls()
        {
            if (!isActive) return;

            foreach (var pair in originalValues)
            {
                var ball = pair.Key;

                if (ball != null && ball.activeInHierarchy &&
                    ball.TryGetComponent<Rigidbody>(out var rb) &&
                    ball.TryGetComponent<SphereCollider>(out var collider))
                {
                    float currentRadius = collider.radius * ball.transform.lossyScale.x;
                    ball.transform.localScale = pair.Value.scale;
                    float restoredRadius = collider.radius * ball.transform.lossyScale.x;

                    float centerShift = restoredRadius - currentRadius;
                    ball.transform.position += Vector3.up * centerShift;

                    rb.mass = pair.Value.mass;
                    rb.linearDamping = pair.Value.drag;
                    rb.angularDamping = pair.Value.angularDrag;

                    var physics = ball.GetComponent<BallPhysics>();
                    physics?.SyncWithCurrentTransform();

                    // возвращаем цвет
                    var renderer = ball.GetComponent<Renderer>();
                    if (renderer != null)
                        renderer.material.color = Color.white;
                }
            }

            originalValues.Clear();
            isActive = false;
        }

        private void OnDisable()
        {
            if (isActive)
                ResetBalls();
        }

        private void ApplyScaleKeepingBallOnTable(GameObject ball, SphereCollider collider, float scaleFactor)
        {
            float oldRadius = collider.radius * ball.transform.lossyScale.x;

            ball.transform.localScale *= scaleFactor;

            float newRadius = collider.radius * ball.transform.lossyScale.x;
            float centerShift = newRadius - oldRadius;
            ball.transform.position += Vector3.up * centerShift;
        }
    }
}
