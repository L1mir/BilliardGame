using UnityEngine;
using System.Collections.Generic;
using DefaultNamespace;

public class AnchoredChaosModifier : GameModifier, IEndOfTurnEffect
{
    [SerializeField] private int minAnchoredBalls = 2;
    [SerializeField] private int maxAnchoredBalls = 5;
    [SerializeField] private Color anchoredColor = Color.gray;
    
    private Dictionary<Rigidbody, Color> originalBalls = new Dictionary<Rigidbody, Color>();
    private List<Rigidbody> allBalls = new List<Rigidbody>();
    
    public void UseModifier()
    {
        Activate();
    }
    
    protected override void Update()
    {
        // disable timer(user turn based)
    }
    
    protected override void OnActivate()
    {
        FindObjectOfType<TurnEffectManager>().Register(this);
        
        var balls = GameObject.FindGameObjectsWithTag("Ball");
        allBalls.Clear();
        
        foreach (var ball in balls)
        {
            var rb = ball.GetComponent<Rigidbody>();
            if (rb != null && !ball.CompareTag("WhiteBall"))
            {
                allBalls.Add(rb);
            }
        }
        
        int ballsToAnchor = Mathf.Min(Random.Range(minAnchoredBalls, maxAnchoredBalls + 1), allBalls.Count);
        
        for (int i = 0; i < ballsToAnchor; i++)
        {
            if (allBalls.Count == 0) break;
            
            int randomIndex = Random.Range(0, allBalls.Count);
            Rigidbody ballRb = allBalls[randomIndex];

            var renderer = ballRb.GetComponent<Renderer>();
            if (renderer != null)
            {
                originalBalls[ballRb] = renderer.material.color;
                renderer.material.color = anchoredColor;
            }

            ballRb.isKinematic = true;
            
            allBalls.RemoveAt(randomIndex);
        }
    }
    
    public void OnTurnEnd()
    {
        Deactivate();
        FindObjectOfType<TurnEffectManager>().Unregister(this);
    }
    
    private System.Collections.IEnumerator UnanchorBallAfterDelay(Rigidbody ballRb, float delay)
    {
        yield return new WaitForSeconds(delay);
        
        if (ballRb != null && originalBalls.ContainsKey(ballRb))
        {
            ballRb.isKinematic = false;
            
            var renderer = ballRb.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material.color = originalBalls[ballRb];
            }
            
            originalBalls.Remove(ballRb);
        }
    }
    
    protected override void OnDeactivate()
    {
        foreach (var kvp in originalBalls)
        {
            if (kvp.Key != null)
            {
                kvp.Key.isKinematic = false;
                
                var renderer = kvp.Key.GetComponent<Renderer>();
                if (renderer != null)
                {
                    renderer.material.color = kvp.Value;
                }
            }
        }
        
        originalBalls.Clear();
        StopAllCoroutines();
    }
}