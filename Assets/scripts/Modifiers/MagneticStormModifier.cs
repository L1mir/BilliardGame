using UnityEngine;
using System.Collections.Generic;

public class MagneticStormModifier : GameModifier
{
    [SerializeField] private float attractionForce = 50f;
    [SerializeField] private float attractionRadius = 3f;
    [SerializeField] private Color magneticEffectColor = Color.blue;
    
    private List<Rigidbody> allBalls = new List<Rigidbody>();
    private Dictionary<Rigidbody, Color> originalColors = new Dictionary<Rigidbody, Color>();

    protected override void OnActivate()
    {
        var balls = GameObject.FindGameObjectsWithTag("Ball");
        allBalls.Clear();
        originalColors.Clear();
        
        foreach (var ball in balls)
        {
            var rb = ball.GetComponent<Rigidbody>();
            if (rb != null && !ball.CompareTag("WhiteBall"))
            {
                allBalls.Add(rb);
                var renderer = ball.GetComponent<Renderer>();
                if (renderer != null)
                {
                    originalColors[rb] = renderer.material.color;
                    renderer.material.color = Color.Lerp(renderer.material.color, magneticEffectColor, 0.5f);
                }
            }
        }
        Debug.Log("MagneticStorm activated! Balls: " + allBalls.Count);
    }

    public void UseMagneticStorm()
    {
        Activate();
    }

    protected override void Update()
    {
        base.Update();
        
        if (!IsActive) return;

        ApplyMagneticForces();
    }

    private void ApplyMagneticForces()
    {
        for (int i = 0; i < allBalls.Count; i++)
        {
            if (allBalls[i] == null) continue;
            
            for (int j = i + 1; j < allBalls.Count; j++)
            {
                if (allBalls[j] == null) continue;
                
                Vector3 direction = allBalls[j].position - allBalls[i].position;
                float distance = direction.magnitude;
                
                if (distance < attractionRadius && distance > 0.1f)
                {
                    float forceMagnitude = attractionForce * Time.deltaTime;
                    allBalls[i].AddForce(direction.normalized * forceMagnitude, ForceMode.Impulse);
                    allBalls[j].AddForce(-direction.normalized * forceMagnitude, ForceMode.Impulse);
                    
                    Debug.DrawLine(allBalls[i].position, allBalls[j].position, 
                        Color.Lerp(Color.blue, Color.red, forceMagnitude / attractionForce));
                }
            }
        }
    }

    protected override void OnDeactivate()
    {
        foreach (var ball in allBalls)
        {
            if (ball != null)
            {
                ball.linearVelocity = Vector3.zero;
                ball.angularVelocity = Vector3.zero;
            }
        }

        foreach (var kvp in originalColors)
        {
            if (kvp.Key != null)
            {
                var renderer = kvp.Key.GetComponent<Renderer>();
                if (renderer != null) renderer.material.color = kvp.Value;
            }
        }

        allBalls.Clear();
        originalColors.Clear();
    
        Debug.Log("MagneticStorm deactivated!");
    }
}