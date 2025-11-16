using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Abilities
{
    public class BallSwapAbility : Ability
    {
        [Tooltip("Ссылка на белый шар")]
        [SerializeField] private GameObject whiteBall;
    
        private const string SOLID_LAYER = "Solid";
        private const string STRIP_LAYER = "Strip";
        private const string WHITE_BALL_TAG = "WhiteBall";
        private const string BALL_TAG = "Ball";

        public void UseBallSwap()
        {
            Debug.Log("UseBallSwap called");
            OnActivate();
        }
        
        protected override void OnActivate()
        {
            if (isActive) 
            {
                Debug.LogWarning("Ability already active!");
                return;
            }
            
            isActive = true;
            Debug.Log("BallSwap ability activated");

            StartCoroutine(SwapBallsCoroutine());
        }

        private IEnumerator SwapBallsCoroutine()
        {

            GameController gameController = FindObjectOfType<GameController>();
            if (gameController == null)
            {
                Debug.LogError("GameController not found!");
                isActive = false;
                yield break;
            }

            Player currentPlayer = gameController.GetPlayer();
            if (currentPlayer == null)
            {
                Debug.LogError("Current player not found!");
                isActive = false;
                yield break;
            }

            Team currentTeam = currentPlayer.GetTeam();
            if (currentTeam == null)
            {
                Debug.LogError("Current team not found!");
                isActive = false;
                yield break;
            }

            Debug.Log($"Current team: {currentTeam.GetTeamType()}");

            if (whiteBall == null)
            {
                whiteBall = GameObject.FindGameObjectWithTag(WHITE_BALL_TAG);
                if (whiteBall == null)
                {
                    Debug.LogError("White ball not found!");
                    isActive = false;
                    yield break;
                }
            }

            GameObject targetBall = FindValidTeamBall(currentTeam);
            if (targetBall == null)
            {
                Debug.LogWarning("No valid team ball found for swapping!");
                isActive = false;
                yield break;
            }

            Debug.Log($"Attempting swap between white ball ({whiteBall.name}) and {targetBall.name}");

            Collider whiteBallCollider = whiteBall.GetComponent<Collider>();
            Collider targetBallCollider = targetBall.GetComponent<Collider>();
            
            bool whiteColliderWasEnabled = whiteBallCollider != null && whiteBallCollider.enabled;
            bool targetColliderWasEnabled = targetBallCollider != null && targetBallCollider.enabled;

            if (whiteBallCollider != null) whiteBallCollider.enabled = false;
            if (targetBallCollider != null) targetBallCollider.enabled = false;

            PerformInstantSwap(whiteBall, targetBall);

            yield return new WaitForSeconds(0.05f);

            if (whiteBallCollider != null) whiteBallCollider.enabled = whiteColliderWasEnabled;
            if (targetBallCollider != null) targetBallCollider.enabled = targetColliderWasEnabled;

            Debug.Log($"Swap completed successfully! White ball at: {whiteBall.transform.position}, Target ball at: {targetBall.transform.position}");

            isActive = false;
        }

        private GameObject FindValidTeamBall(Team currentTeam)
        {
            string targetLayer = currentTeam.GetTeamType() == TeamType.Solid ? SOLID_LAYER : STRIP_LAYER;
            int targetLayerMask = LayerMask.NameToLayer(targetLayer);

            Debug.Log($"Looking for balls in layer: {targetLayer} (mask: {targetLayerMask})");

            GameObject[] allBalls = GameObject.FindGameObjectsWithTag(BALL_TAG);
            Debug.Log($"Found {allBalls.Length} balls with tag {BALL_TAG}");

            List<GameObject> validBalls = new List<GameObject>();

            foreach (GameObject ball in allBalls)
            {
                if (ball == null) continue;
                
                Debug.Log($"Checking ball: {ball.name}, Layer: {ball.layer}, Tag: {ball.tag}, Active: {ball.activeInHierarchy}");

                if (ball != whiteBall && 
                    !ball.CompareTag(WHITE_BALL_TAG) && 
                    ball.layer == targetLayerMask &&
                    ball.activeInHierarchy)
                {
                    validBalls.Add(ball);
                    Debug.Log($"Valid ball found: {ball.name}");
                }
            }

            Debug.Log($"Found {validBalls.Count} valid balls for team {targetLayer}");
            
            if (validBalls.Count == 0)
            {
                Debug.LogWarning($"No balls found for team: {targetLayer}");
                return null;
            }

            GameObject selectedBall = validBalls[Random.Range(0, validBalls.Count)];
            Debug.Log($"Selected ball for swap: {selectedBall.name}");
            return selectedBall;
        }

        private void PerformInstantSwap(GameObject ball1, GameObject ball2)
        {
            if (ball1 == null || ball2 == null)
            {
                Debug.LogError("One of the balls is null in swap!");
                return;
            }

            Debug.Log($"Swapping positions: {ball1.name} at {ball1.transform.position} with {ball2.name} at {ball2.transform.position}");

            Vector3 pos1 = ball1.transform.position;
            Vector3 pos2 = ball2.transform.position;
            Quaternion rot1 = ball1.transform.rotation;
            Quaternion rot2 = ball2.transform.rotation;

            StopAllPhysics(ball1);
            StopAllPhysics(ball2);

            ball1.transform.SetPositionAndRotation(pos2, rot2);
            ball2.transform.SetPositionAndRotation(pos1, rot1);

            Debug.Log($"After swap: {ball1.name} at {ball1.transform.position}, {ball2.name} at {ball2.transform.position}");
        }

        private void StopAllPhysics(GameObject ball)
        {
            Rigidbody rb = ball.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
                
                if (!rb.isKinematic)
                {
                    rb.isKinematic = true;
                    StartCoroutine(ReenablePhysicsAfterDelay(rb, 0.1f));
                }
                else
                {
                    rb.Sleep();
                }
            }
        }

        private IEnumerator ReenablePhysicsAfterDelay(Rigidbody rb, float delay)
        {
            yield return new WaitForSeconds(delay);
            if (rb != null)
            {
                rb.isKinematic = false;
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }
        }
        
    }
}