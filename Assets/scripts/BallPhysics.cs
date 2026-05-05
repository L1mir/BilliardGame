using UnityEngine;

[RequireComponent(typeof(Rigidbody), typeof(SphereCollider))]
public class BallPhysics : MonoBehaviour
{
    private Rigidbody rb;

    public bool isAnchored;

    [Header("Rigidbody Setup")]
    [SerializeField] private bool overrideRigidbodySettings = true;
    [SerializeField] private float ballMass = 0.165f;
    [SerializeField] private float linearDamping = 0.02f;
    [SerializeField] private float angularDamping = 0.05f;
    [SerializeField] private float maxAngularSpeed = 120f;

    [Header("Cloth Model")]
    [SerializeField] private float slidingFrictionAcceleration = 1.7f;
    [SerializeField] private float rollingResistanceAcceleration = 0.3f;
    [SerializeField] private float slipToRollThreshold = 0.09f;
    [SerializeField] private float rollSpinCatchUp = 16f;
    [SerializeField] private float sideSpinDecay = 2f;

    [Header("Pocket Capture")]
    [SerializeField] private float pocketCaptureDepth = 0.012f;
    [SerializeField] private float pocketGravityBoost = 22f;
    [SerializeField] private float pocketHorizontalDamping = 3.5f;

    [Header("Stop Thresholds")]
    [SerializeField] private float stopSpeed = 0.045f;
    [SerializeField] private float stopAngularSpeed = 0.35f;
    [SerializeField] private float verticalLockSpeed = 0.08f;
    [SerializeField] private float tableSnapTolerance = 0.004f;
    [SerializeField] private float postStrikeNoSleepTime = 0.12f;

    private float radius = 0.028575f;
    private float tableSurfaceY;
    private float noSleepTimer;

    public float Radius => radius;

    public bool IsEffectivelyStopped =>
        rb != null &&
        rb.linearVelocity.sqrMagnitude <= stopSpeed * stopSpeed &&
        rb.angularVelocity.sqrMagnitude <= stopAngularSpeed * stopAngularSpeed;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        RefreshRadius();

        tableSurfaceY = transform.position.y;

        if (overrideRigidbodySettings)
        {
            ApplyRecommendedRigidbodySettings();
        }
    }

    private void OnEnable()
    {
        if (rb == null)
        {
            rb = GetComponent<Rigidbody>();
        }
    }

    private void FixedUpdate()
    {
        if (rb == null)
        {
            return;
        }

        if (noSleepTimer > 0f)
        {
            noSleepTimer -= Time.fixedDeltaTime;
        }

        if (rb.isKinematic)
        {
            return;
        }

        if (isAnchored)
        {
            FreezeMotion();
            return;
        }

        if (IsInsidePocketCaptureZone())
        {
            ApplyPocketCapture();
            TrySleep();
            return;
        }

        LockTinyVerticalBounce();
        ApplyClothResponse();
        TrySleep();
    }

    public void ApplyCueStrike(Vector3 direction, float impulse, float followSpinRatio = 0.08f)
    {
        if (rb == null)
        {
            rb = GetComponent<Rigidbody>();
        }

        direction = Vector3.ProjectOnPlane(direction, Vector3.up).normalized;
        if (direction.sqrMagnitude <= Mathf.Epsilon)
        {
            return;
        }

        rb.WakeUp();
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        noSleepTimer = postStrikeNoSleepTime;

        float launchSpeed = impulse / Mathf.Max(rb.mass, 0.0001f);
        rb.linearVelocity = direction * launchSpeed;

        if (followSpinRatio > 0f)
        {
            Vector3 spinAxis = Vector3.Cross(Vector3.up, direction);
            rb.angularVelocity = spinAxis * (launchSpeed * followSpinRatio / Mathf.Max(radius, 0.0001f));
        }
    }

    public void RecalibrateSurfaceHeight()
    {
        tableSurfaceY = transform.position.y;
    }

    public void SyncWithCurrentTransform(bool recalibrateSurfaceHeight = true, float noSleepDuration = 0.05f)
    {
        RefreshRadius();

        if (recalibrateSurfaceHeight)
        {
            tableSurfaceY = transform.position.y;
        }

        noSleepTimer = Mathf.Max(noSleepTimer, noSleepDuration);

        if (rb != null && !rb.isKinematic)
        {
            rb.WakeUp();
        }
    }

    public void ResetMotion(bool sleep = false)
    {
        if (rb == null)
        {
            rb = GetComponent<Rigidbody>();
        }

        if (rb == null || rb.isKinematic)
        {
            return;
        }

        FreezeMotion();

        if (sleep)
        {
            rb.Sleep();
        }
    }

    private void ApplyRecommendedRigidbodySettings()
    {
        rb.mass = ballMass;
        rb.linearDamping = linearDamping;
        rb.angularDamping = angularDamping;
        rb.useGravity = true;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        rb.maxAngularVelocity = maxAngularSpeed;
    }

    private void ApplyClothResponse()
    {
        Vector3 planarVelocity = Vector3.ProjectOnPlane(rb.linearVelocity, Vector3.up);
        if (planarVelocity.sqrMagnitude <= 0.000001f)
        {
            return;
        }

        Vector3 contactVelocity = planarVelocity + Vector3.Cross(rb.angularVelocity, -Vector3.up * radius);
        Vector3 slip = Vector3.ProjectOnPlane(contactVelocity, Vector3.up);
        float slipSpeed = slip.magnitude;

        if (slipSpeed > slipToRollThreshold)
        {
            ApplySlidingFriction(slip / slipSpeed);
            return;
        }

        ApplyRollingResistance(planarVelocity);
        SyncSpinToRolling(planarVelocity);
    }

    private void ApplySlidingFriction(Vector3 slipDirection)
    {
        rb.AddForce(-slipDirection * slidingFrictionAcceleration, ForceMode.Acceleration);

        float angularAcceleration = (5f * slidingFrictionAcceleration) / (2f * Mathf.Max(radius, 0.0001f));
        rb.AddTorque(Vector3.Cross(Vector3.up, slipDirection) * angularAcceleration, ForceMode.Acceleration);

        Vector3 sideSpin = Vector3.Project(rb.angularVelocity, Vector3.up);
        sideSpin = Vector3.MoveTowards(sideSpin, Vector3.zero, sideSpinDecay * Time.fixedDeltaTime);
        rb.angularVelocity = (rb.angularVelocity - Vector3.Project(rb.angularVelocity, Vector3.up)) + sideSpin;
    }

    private void ApplyRollingResistance(Vector3 planarVelocity)
    {
        float speed = planarVelocity.magnitude;
        if (speed <= 0.0001f)
        {
            return;
        }

        rb.AddForce(-(planarVelocity / speed) * rollingResistanceAcceleration, ForceMode.Acceleration);
    }

    private void SyncSpinToRolling(Vector3 planarVelocity)
    {
        Vector3 sideSpin = Vector3.Project(rb.angularVelocity, Vector3.up);
        Vector3 currentRollingSpin = rb.angularVelocity - sideSpin;
        Vector3 desiredRollingSpin = Vector3.Cross(Vector3.up, planarVelocity) / Mathf.Max(radius, 0.0001f);

        currentRollingSpin = Vector3.MoveTowards(
            currentRollingSpin,
            desiredRollingSpin,
            rollSpinCatchUp * Time.fixedDeltaTime
        );

        sideSpin = Vector3.MoveTowards(sideSpin, Vector3.zero, sideSpinDecay * Time.fixedDeltaTime);
        rb.angularVelocity = currentRollingSpin + sideSpin;
    }

    private void LockTinyVerticalBounce()
    {
        if (Mathf.Abs(rb.linearVelocity.y) > verticalLockSpeed)
        {
            return;
        }

        if (transform.position.y < tableSurfaceY - tableSnapTolerance)
        {
            return;
        }

        Vector3 velocity = rb.linearVelocity;
        velocity.y = 0f;
        rb.linearVelocity = velocity;
    }

    private bool IsInsidePocketCaptureZone()
    {
        return transform.position.y < tableSurfaceY - pocketCaptureDepth;
    }

    private void ApplyPocketCapture()
    {
        Vector3 velocity = rb.linearVelocity;
        Vector3 planarVelocity = new Vector3(velocity.x, 0f, velocity.z);

        float damping = Mathf.Clamp01(pocketHorizontalDamping * Time.fixedDeltaTime);
        planarVelocity = Vector3.Lerp(planarVelocity, Vector3.zero, damping);

        velocity.x = planarVelocity.x;
        velocity.z = planarVelocity.z;
        rb.linearVelocity = velocity;

        rb.AddForce(Vector3.down * pocketGravityBoost, ForceMode.Acceleration);
        rb.angularVelocity = Vector3.Lerp(rb.angularVelocity, Vector3.zero, damping);
    }

    private void TrySleep()
    {
        if (noSleepTimer > 0f)
        {
            return;
        }

        if (!IsEffectivelyStopped)
        {
            return;
        }

        FreezeMotion();
        rb.Sleep();
    }

    private void FreezeMotion()
    {
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
    }

    private void RefreshRadius()
    {
        if (TryGetComponent<SphereCollider>(out var sphereCollider))
        {
            radius = sphereCollider.radius * transform.lossyScale.x;
            return;
        }

        radius = transform.lossyScale.x * 0.5f;
    }

    private void OnDrawGizmosSelected()
    {
        RefreshRadius();

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}
