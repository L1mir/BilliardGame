using UnityEngine;

public class BallPhysics : MonoBehaviour
{
    private Rigidbody rb;
    public bool isAnchored;
    [SerializeField] private float rollingFriction = 0.4f;
    [SerializeField] private float stopThreshold = 0.05f;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        if (rb.isKinematic) return;
        
        if (isAnchored)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            return;
        }
        
        Vector3 v = rb.linearVelocity;
        if (v.magnitude > 0)
        {
            Vector3 friction = -v * rollingFriction * Time.fixedDeltaTime;
            if (friction.magnitude > v.magnitude) rb.linearVelocity = Vector3.zero;
            else rb.linearVelocity += friction;
        }
        if (v.magnitude > 0.1f)
        {
            float radius = 0.028f;
            Vector3 spinAxis = Vector3.Cross(Vector3.up, v.normalized);

            rb.angularVelocity += spinAxis * (v.magnitude / radius) * 0.015f * Time.fixedDeltaTime;
        }

        if (rb.linearVelocity.magnitude < stopThreshold)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
    }
}