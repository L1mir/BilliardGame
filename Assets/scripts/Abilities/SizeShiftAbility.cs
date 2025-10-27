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
        private bool isActive = false;
        
        protected override void OnActivate()
        {
            var gameController = FindObjectOfType<GameController>();
            if (gameController == null) return;
            
            isActive = true;
            var allBalls = GameObject.FindGameObjectsWithTag("Ball");
            
            foreach (var ball in allBalls)
            {
                if (ball.activeInHierarchy && ball.TryGetComponent<Rigidbody>(out var rb))
                {
                    // Сохраняем оригинальные значения (кроме массы)
                    originalValues[ball] = (
                        ball.transform.localScale,
                        rb.linearDamping,
                        rb.angularDamping
                    );
                    
                    // Уменьшаем шар, но оставляем массу неизменной
                    ball.transform.localScale *= shrinkFactor;
                    // rb.mass остается прежней - не изменяем массу
                    rb.linearDamping *= 0.5f; // Уменьшаем сопротивление
                    rb.angularDamping *= 0.5f; // Уменьшаем угловое сопротивление
                    
                    // Обновляем коллайдер
                    if (ball.TryGetComponent<SphereCollider>(out var collider))
                    {
                        collider.radius = 0.5f; // Возвращаем радиус к стандартному значению (1.0f * shrinkFactor)
                    }
                }
            }
            
            // Запускаем таймер для автоматического сброса через duration секунд
            Invoke(nameof(ResetBalls), duration);
        }
        
        private void Update()
        {
            // Убрали проверку нажатия E, так как это конфликтует с логикой удара
            // Способность теперь сбрасывается автоматически через duration секунд
        }
        
        private void ResetBalls()
        {
            if (!isActive) return;
            
            foreach (var pair in originalValues)
            {
                var ball = pair.Key;
                if (ball != null && ball.activeInHierarchy && ball.TryGetComponent<Rigidbody>(out var rb))
                {
                    // Восстанавливаем оригинальные значения (кроме массы)
                    ball.transform.localScale = pair.Value.scale;
                    // rb.mass не восстанавливаем, так как не изменяли
                    rb.linearDamping = pair.Value.drag;
                    rb.angularDamping = pair.Value.angularDrag;
                    
                    // Восстанавливаем коллайдер
                    if (ball.TryGetComponent<SphereCollider>(out var collider))
                    {
                        collider.radius = 0.5f; // Стандартный радиус для шара
                    }
                }
            }
            
            originalValues.Clear();
            isActive = false;
            Debug.Log("Способность SizeShift сброшена - размеры шаров восстановлены");
        }
        
        private void OnDisable()
        {
            // При отключении компонента сбрасываем все изменения
            if (isActive)
            {
                ResetBalls();
            }
        }
    }
}