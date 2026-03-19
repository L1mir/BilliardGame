using UnityEngine;

public class RollingFriction : MonoBehaviour
{
    private Rigidbody rb;

    [Header("Настройки трения качения")]
    public float frictionCoefficient = 0.0005f;
    public float stopThreshold = 0.05f;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        if (rb == null || rb.IsSleeping()) return;

        float speed = rb.linearVelocity.magnitude;
        
        if (speed < stopThreshold)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.Sleep();
            return;
        }
        
        Vector3 friction = -rb.linearVelocity.normalized 
                           * frictionCoefficient 
                           * Physics.gravity.magnitude 
                           * rb.mass;

        rb.AddForce(friction, ForceMode.Force);
    }
}