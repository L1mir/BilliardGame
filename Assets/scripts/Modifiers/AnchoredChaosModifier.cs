using UnityEngine;
using System.Collections.Generic;
using DefaultNamespace;

public class AnchoredChaosModifier : GameModifier, IEndOfTurnEffect
{
    [SerializeField, Range(0f, 100f)] private float anchoredBallPercentage = 25f;
    [SerializeField] private Color anchoredColor = Color.gray;

    private readonly Dictionary<Rigidbody, BallStateData> anchoredBalls = new();
    private readonly List<Rigidbody> availableBalls = new();
    private static readonly int ColorID = Shader.PropertyToID("_Color");
    private MaterialPropertyBlock propBlock;

    private TurnEffectManager turnManager;

    private struct BallStateData
    {
        public Color originalColor;
        public RigidbodyConstraints originalConstraints;
        public Vector3 originalVelocity;
        public Vector3 originalAngularVelocity;
        public Renderer renderer;
    }

    public void UseModifier()
    {
        Activate();
    }

    protected void Awake()
    {
        turnManager = FindObjectOfType<TurnEffectManager>();
    }

    protected override void Update()
    {
        // turn-based, ничего не делаем
    }

    protected override void OnActivate()
    {
        if (turnManager != null)
            turnManager.Register(this);

        anchoredBalls.Clear();
        availableBalls.Clear();

        var balls = GameObject.FindGameObjectsWithTag("Ball");

        foreach (var ball in balls)
        {
            if (ball.CompareTag("WhiteBall")) continue;

            var rb = ball.GetComponent<Rigidbody>();
            if (rb == null) continue;

            availableBalls.Add(rb);
        }

        if (availableBalls.Count == 0) return;

        int count = CalculateAnchoredBallCount(availableBalls.Count);

        for (int i = 0; i < count; i++)
        {
            int index = Random.Range(0, availableBalls.Count);
            var rb = availableBalls[index];
            availableBalls.RemoveAt(index);

            if (rb == null) continue;

            AnchorBall(rb);
        }
    }

    private int CalculateAnchoredBallCount(int totalBallsOnTable)
    {
        if (totalBallsOnTable <= 0)
        {
            return 0;
        }

        float normalizedPercentage = Mathf.Clamp(anchoredBallPercentage, 0f, 100f);
        float rawCount = totalBallsOnTable * normalizedPercentage / 100f;

        return Mathf.Clamp(Mathf.CeilToInt(rawCount), 0, totalBallsOnTable);
    }

    private void AnchorBall(Rigidbody rb)
    {
        if (anchoredBalls.ContainsKey(rb)) return;

        var renderer = rb.GetComponent<Renderer>();
        var physics = rb.GetComponent<BallPhysics>();

        if (physics == null)
        {
            Debug.LogWarning("BallPhysics missing!");
            return;
        }

        BallStateData data = new BallStateData
        {
            originalConstraints = rb.constraints,
            originalVelocity = rb.linearVelocity,
            originalAngularVelocity = rb.angularVelocity,
            renderer = renderer,
            originalColor = renderer != null ? renderer.material.color : Color.white
        };

        anchoredBalls.Add(rb, data);
        
        physics.isAnchored = true;

        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        if (renderer != null)
        {
            if (propBlock == null) propBlock = new MaterialPropertyBlock();
            renderer.GetPropertyBlock(propBlock);
            propBlock.SetColor("_BaseColor", anchoredColor);
            renderer.SetPropertyBlock(propBlock);
        }
    }

    public void OnTurnEnd()
    {
        Deactivate();

        if (turnManager != null)
            turnManager.Unregister(this);
    }

    protected override void OnDeactivate()
    {
        foreach (var kvp in anchoredBalls)
        {
            var rb = kvp.Key;
            var data = kvp.Value;

            if (rb == null) continue;

            var physics = rb.GetComponent<BallPhysics>();

            if (physics != null)
                physics.isAnchored = false;

            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;

            if (data.renderer != null)
            {
                data.renderer.GetPropertyBlock(propBlock);
                propBlock.SetColor("_BaseColor", data.originalColor);
                data.renderer.SetPropertyBlock(propBlock);
            }
        }

        anchoredBalls.Clear();
    }
}
