using UnityEngine;

public class BallPhysics : MonoBehaviour
{
    private Rigidbody rb;

    [SerializeField] private float rollingFriction = 0.4f;
    [SerializeField] private float stopThreshold = 0.05f;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        Vector3 v = rb.linearVelocity;

        if (v.magnitude > 0)
        {
            Vector3 friction = -v.normalized * rollingFriction * Time.fixedDeltaTime;
            
            if (friction.magnitude > v.magnitude)
                rb.linearVelocity = Vector3.zero;
            else
                rb.linearVelocity += friction;
        }

        if (rb.linearVelocity.magnitude < stopThreshold)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
    }
}