using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Abilities
{
    public class BallSwapAbility : Ability
    {
        [SerializeField] private GameObject whiteBall;

        private const string SOLID_LAYER = "Solid";
        private const string STRIP_LAYER = "Strip";
        private const string WHITE_BALL_TAG = "WhiteBall";
        private const string BALL_TAG = "Ball";

        private GameController gameController;

        private void Awake()
        {
            abilityCost = 0;
            gameController = FindObjectOfType<GameController>();
        }

        public void UseBallSwap()
        {
            if (!isActive)
                Activate();
        }

        protected override void OnActivate()
        {
            if (gameController == null)
            {
                Debug.LogError("GameController not found!");
                isActive = false;
                return;
            }

            Player currentPlayer = gameController.GetPlayer();
            if (currentPlayer == null)
            {
                Debug.LogError("Player not found!");
                isActive = false;
                return;
            }

            if (currentPlayer.AbilityPoints < abilityCost)
            {
                Debug.Log("Not enough points!");
                isActive = false;
                return;
            }

            currentPlayer.AbilityPoints -= abilityCost;
            gameController.ShowCurrentPlayerInfo();

            StartCoroutine(SwapCoroutine(currentPlayer));
        }

        private IEnumerator SwapCoroutine(Player player)
        {
            // Найти белый шар если не задан
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

            GameObject targetBall = FindValidTeamBall(player.GetTeam());

            if (targetBall == null)
            {
                Debug.LogWarning("No valid ball to swap!");
                isActive = false;
                yield break;
            }
            
            if (targetBall == whiteBall)
            {
                Debug.LogWarning("Tried to swap with itself!");
                isActive = false;
                yield break;
            }

            Rigidbody rb1 = whiteBall.GetComponent<Rigidbody>();
            Rigidbody rb2 = targetBall.GetComponent<Rigidbody>();

            Collider col1 = whiteBall.GetComponent<Collider>();
            Collider col2 = targetBall.GetComponent<Collider>();

            if (rb1 == null || rb2 == null)
            {
                Debug.LogError("Missing Rigidbody!");
                isActive = false;
                yield break;
            }

            if (Vector3.Distance(rb1.position, rb2.position) < 0.001f)
            {
                Debug.LogWarning("Balls are too close - swap skipped");
                isActive = false;
                yield break;
            }

            rb1.linearVelocity = Vector3.zero;
            rb2.linearVelocity = Vector3.zero;
            rb1.angularVelocity = Vector3.zero;
            rb2.angularVelocity = Vector3.zero;

            rb1.isKinematic = true;
            rb2.isKinematic = true;

            if (col1 != null) col1.enabled = false;
            if (col2 != null) col2.enabled = false;

            yield return null;

            Vector3 pos1 = rb1.position;
            Vector3 pos2 = rb2.position;

            Quaternion rot1 = rb1.rotation;
            Quaternion rot2 = rb2.rotation;

            rb1.position = pos2;
            rb2.position = pos1;

            rb1.rotation = rot2;
            rb2.rotation = rot1;

            Physics.SyncTransforms();

            yield return new WaitForSeconds(0.02f);

            rb1.isKinematic = false;
            rb2.isKinematic = false;

            rb1.linearVelocity = Vector3.zero;
            rb2.linearVelocity = Vector3.zero;
            rb1.angularVelocity = Vector3.zero;
            rb2.angularVelocity = Vector3.zero;

            if (col1 != null) col1.enabled = true;
            if (col2 != null) col2.enabled = true;

            isActive = false;
        }

        private GameObject FindValidTeamBall(Team team)
        {
            string layerName = team.GetTeamType() == TeamType.Solid ? SOLID_LAYER : STRIP_LAYER;
            int layer = LayerMask.NameToLayer(layerName);

            GameObject[] balls = GameObject.FindGameObjectsWithTag(BALL_TAG);
            List<GameObject> valid = new List<GameObject>();

            foreach (var ball in balls)
            {
                if (ball == null) continue;

                if (ball != whiteBall &&
                    !ball.CompareTag(WHITE_BALL_TAG) &&
                    ball.layer == layer &&
                    ball.activeInHierarchy)
                {
                    valid.Add(ball);
                }
            }

            if (valid.Count == 0) return null;

            return valid[Random.Range(0, valid.Count)];
        }
    }
}