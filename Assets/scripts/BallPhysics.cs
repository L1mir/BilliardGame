using UnityEngine;

public class BallPhysics : MonoBehaviour
{
    private Rigidbody rb;
    public bool isAnchored;
    
    [Header("Physical Properties")]
    [SerializeField] private float radius = 0.028f;
    [SerializeField] private float mass = 0.01f;
    [SerializeField] private float rollingFrictionCoeff = 0.008f;
    [SerializeField] private float stopThreshold = 0.03f;
    
    [Header("Ground Properties")]
    [SerializeField] private float groundNormalForce = 9.81f;
    [SerializeField] private float airResistance = 0.998f;
    
    private void Awake()
    {
        rb = GetComponent<Rigidbody>();

        if (TryGetComponent<SphereCollider>(out var sphereCollider))
        {
            radius = sphereCollider.radius;
        }

        if (radius <= 0)
        {
            radius = transform.localScale.x / 2f;
        }
    }
    
    private void FixedUpdate()
    {
        if (rb.isKinematic || isAnchored) 
        {
            if (isAnchored)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }
            return;
        }
        
        Vector3 velocity = rb.linearVelocity;
        float speed = velocity.magnitude;
        
        if (speed > stopThreshold)
        {
            velocity *= airResistance;

            // F_rolling = μ_rolling * N / r  (момент силы)
            float rollingForce = rollingFrictionCoeff * groundNormalForce / radius;
            Vector3 frictionForce = -velocity.normalized * rollingForce * Time.fixedDeltaTime;
            
            if (frictionForce.magnitude > speed)
            {
                velocity = Vector3.zero;
            }
            else
            {
                velocity += frictionForce;
            }

            if (speed > 0.01f)
            {
                Vector3 desiredAngularVelocity = Vector3.Cross(Vector3.up, velocity.normalized) * (speed / radius);
                rb.angularVelocity = Vector3.Lerp(rb.angularVelocity, desiredAngularVelocity, 0.07f);
            }
            
            rb.linearVelocity = velocity;
        }
        else
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
    }
 
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}