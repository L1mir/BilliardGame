using UnityEngine;
using System.Collections.Generic;

public class MagneticStormModifier : GameModifier
{
    [SerializeField] private float attractionForce = 7.5f;
    [SerializeField] private float attractionRadius = 3f;
    [SerializeField] private float startOnBallSpeed = 0.08f;
    // [SerializeField] private Color magneticEffectColor = Color.blue;
    
    private List<Rigidbody> allBalls = new List<Rigidbody>();
    // private Dictionary<Rigidbody, Color> originalColors = new Dictionary<Rigidbody, Color>();
    private bool stormStarted;

    protected override void OnActivate()
    {
        var balls = GameObject.FindGameObjectsWithTag("Ball");
        allBalls.Clear();
        stormStarted = false;
        // originalColors.Clear();
        
        foreach (var ball in balls)
        {
            var rb = ball.GetComponent<Rigidbody>();
            if (rb != null && !ball.CompareTag("WhiteBall"))
            {
                allBalls.Add(rb);
                rb.WakeUp();
                ball.GetComponent<BallPhysics>()?.SyncWithCurrentTransform(false, 0.2f);
                // var renderer = ball.GetComponent<Renderer>();
                // if (renderer != null)
                // {
                //     originalColors[rb] = renderer.material.color;
                //     renderer.material.color = Color.Lerp(renderer.material.color, magneticEffectColor, 0.5f);
                // }
            }
        }
    }

    public void UseMagneticStorm()
    {
        Activate();
    }

    protected override void Update()
    {
        if (!IsActive) return;

        if (!stormStarted)
        {
            if (HasMovingBalls())
            {
                stormStarted = true;
            }
            else
            {
                return;
            }
        }

        remainingTime -= Time.deltaTime;
        if (remainingTime <= 0f)
        {
            Deactivate();
        }
    }

    private void FixedUpdate()
    {
        if (!IsActive || !stormStarted) return;

        ApplyMagneticForces();
    }

    private void ApplyMagneticForces()
    {
        for (int i = 0; i < allBalls.Count; i++)
        {
            if (allBalls[i] == null || !allBalls[i].gameObject.activeInHierarchy || allBalls[i].isKinematic) continue;
            allBalls[i].WakeUp();
            
            for (int j = i + 1; j < allBalls.Count; j++)
            {
                if (allBalls[j] == null || !allBalls[j].gameObject.activeInHierarchy || allBalls[j].isKinematic) continue;
                allBalls[j].WakeUp();
                
                Vector3 direction = allBalls[j].position - allBalls[i].position;
                float distance = direction.magnitude;
                
                if (distance < attractionRadius && distance > 0.1f)
                {
                    float falloff = 1f - (distance / attractionRadius);
                    float acceleration = attractionForce * falloff;
                    Vector3 force = direction.normalized * acceleration;

                    allBalls[i].AddForce(force, ForceMode.Acceleration);
                    allBalls[j].AddForce(-force, ForceMode.Acceleration);
                    
                    // Debug.DrawLine(allBalls[i].position, allBalls[j].position, 
                    //     Color.Lerp(Color.blue, Color.red, forceMagnitude / attractionForce));
                }
            }
        }
    }

    protected override void OnDeactivate()
    {
        // foreach (var kvp in originalColors)
        // {
        //     if (kvp.Key != null)
        //     {
        //         var renderer = kvp.Key.GetComponent<Renderer>();
        //         if (renderer != null) renderer.material.color = kvp.Value;
        //     }
        // }

        allBalls.Clear();
        // originalColors.Clear();
    }

    private bool HasMovingBalls()
    {
        float speedSq = startOnBallSpeed * startOnBallSpeed;

        foreach (var rb in allBalls)
        {
            if (rb == null || !rb.gameObject.activeInHierarchy || rb.isKinematic)
            {
                continue;
            }

            if (rb.linearVelocity.sqrMagnitude >= speedSq)
            {
                return true;
            }
        }

        return false;
    }
}
