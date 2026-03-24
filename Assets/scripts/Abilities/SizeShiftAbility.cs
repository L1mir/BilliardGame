using System.Collections.Generic;
using DefaultNamespace;
using UnityEngine;

namespace Abilities
{
    public class SizeShiftAbility : Ability, IEndOfTurnEffect
    {
        [SerializeField] private float shrinkFactor = 0.5f;

        private Dictionary<GameObject, (Vector3 scale, float mass, float drag, float angularDrag)> originalValues = new();

        protected override void OnActivate()
        {
            Debug.Log("SizeShift OnActivate вызван");

            var turnManager = FindObjectOfType<TurnEffectManager>();
            if (turnManager != null)
                turnManager.Register(this);

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
                    
                    float oldRadius = collider.radius * ball.transform.localScale.x;
                    
                    ball.transform.localScale *= shrinkFactor;

                    float newRadius = collider.radius * ball.transform.localScale.x;

                    float delta = oldRadius - newRadius;
                    ball.transform.position += Vector3.up * delta;

                    rb.mass *= shrinkFactor;          // легче → быстрее
                    rb.linearDamping *= 0.9f;         // чуть быстрее катится
                    rb.angularDamping *= 0.9f;
                }
            }

            Debug.Log("SizeShift применён");
        }

        public void ActivateSizeShiftAbility()
        {
            Debug.Log("Кнопка способности нажата");

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
                    ball.TryGetComponent<Rigidbody>(out var rb))
                {
                    ball.transform.localScale = pair.Value.scale;

                    rb.mass = pair.Value.mass;
                    rb.linearDamping = pair.Value.drag;
                    rb.angularDamping = pair.Value.angularDrag;

                    // возвращаем цвет
                    var renderer = ball.GetComponent<Renderer>();
                    if (renderer != null)
                        renderer.material.color = Color.white;
                }
            }

            originalValues.Clear();
            isActive = false;

            Debug.Log("SizeShift сброшен");
        }

        private void OnDisable()
        {
            if (isActive)
                ResetBalls();
        }
    }
}