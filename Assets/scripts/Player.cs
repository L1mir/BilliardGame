using UnityEngine;
using Unity.Cinemachine;
using System;
using System.Collections;
using Abilities;
using Random = UnityEngine.Random;

public enum TeamType
{
    Strip = 6,
    Solid = 7,
    Unknown = 8
}

public class Player : MonoBehaviour
{
    [SerializeField] private Team team;
    [SerializeField] private GameObject stick_prefab;
    [SerializeField] private CinemachineCamera camera_prefab;

    [Header("Values of Rotation of stick")] [NonSerialized]
    private float stick_rotation_velocity = 2f;

    [SerializeField] private float smooth_time = 0.1f;
    [SerializeField] private float max_speed = 120f;
    [SerializeField] private float strike_force = 5f;
    [SerializeField] private float min_force = 2f;
    [SerializeField] private float max_force = 10f;
    [SerializeField] private float force_changing_factor = 0.5f;
    [SerializeField] private float zoom_changing_factor = 10f;
    [SerializeField] private float min_zoom = 10f;
    [SerializeField] private float max_zoom = 120f;

    [Header("Audio")] [SerializeField] private AudioClip strikeSound;
    [SerializeField] private float strikeSoundVolume = 1f;

    private AudioSource audioSource;

    private int balls_scored = 0;
    private GameController gc;
    private GameObject whiteBall;
    private Animator stick_animator;

    private int abilityPoints = 0;

    public int AbilityPoints
    {
        get => abilityPoints;
        set => abilityPoints = value;
    }

    [Header("Animation parameters")] [SerializeField]
    private float strikeAnimationTime = 1.15f;

    [SerializeField] private float cooldownAfterStrike = 1.5f;
    private bool isStroke = false;
    private bool _isCurrentPlayer = false;
    [NonSerialized] public Vector3 previousBallPosition;

    private void Start()
    {
        gc = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameController>();
        whiteBall = GameObject.FindGameObjectWithTag("WhiteBall");

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        audioSource.playOnAwake = false;
    }

    public bool isCurrentPlayer
    {
        get => _isCurrentPlayer;
        set
        {
            _isCurrentPlayer = value;
            gameObject.SetActive(value);

            if (IsPrefab(stick_prefab))
            {
                stick_prefab = Instantiate(stick_prefab, camera_prefab.transform.position,
                    stick_prefab.transform.rotation, gameObject.transform);
                stick_animator = stick_prefab.GetComponentInChildren<Animator>();
            }

            if (IsPrefab(camera_prefab.gameObject))
            {
                camera_prefab = Instantiate(camera_prefab, camera_prefab.transform.position,
                    camera_prefab.transform.rotation, gameObject.transform);
            }

            stick_prefab.SetActive(value);
            if (value) FollowBall();
        }
    }

    public void PlayStrikeSound()
    {
        if (strikeSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(strikeSound, strikeSoundVolume);
        }
        else
        {
            Debug.LogWarning("Strike sound or AudioSource is missing!");
        }
    }

    private bool IsPrefab(GameObject obj)
    {
        if (obj == null) return false;

        // Check if object is a prefab by verifying if it's in a valid scene
        // Prefabs are not part of any scene, instantiated objects are
        return !obj.scene.IsValid();
    }

    private void Awake()
    {
        gc = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameController>();
    }

    private void Update()
    {
        if (!isStroke && _isCurrentPlayer) RotateStickAroundTheBall();

        CinemachineFollowZoom followZoom = camera_prefab.GetComponent<CinemachineFollowZoom>();
        AdjustValue(Input.GetAxis("Horizontal"), ref followZoom.FovRange.y, min_zoom, max_zoom,
            zoom_changing_factor, UIController.Instance.UpdateCameraZoomPrecents);
        AdjustValue(Input.GetAxis("Vertical"), ref strike_force, min_force, max_force,
            force_changing_factor, UIController.Instance.UpdateScrollBar);
    }

    private void RotateStickAroundTheBall()
    {
        if (whiteBall == null) return;

        stick_prefab.transform.position = whiteBall.transform.position;

        stick_prefab.transform.rotation = Quaternion.Euler(
            -12,
            Mathf.SmoothDampAngle(
                stick_prefab.transform.eulerAngles.y,
                180 + camera_prefab.transform.rotation.eulerAngles.y,
                ref stick_rotation_velocity,
                smooth_time,
                max_speed
            ),
            0
        );
    }

    public IEnumerator MakeStrike()
    {
        if (whiteBall == null)
        {
            Debug.LogError("White ball not found!");
            yield break;
        }

        previousBallPosition = whiteBall.transform.position;

        if (isStroke)
            yield break;

        isStroke = true;

        stick_animator.SetBool("isStroke", true);
        yield return new WaitForSeconds(strikeAnimationTime);

        Vector3 dir = -stick_prefab.transform.forward;
        dir = Vector3.ProjectOnPlane(dir, Vector3.up).normalized;

        Rigidbody rb = whiteBall.GetComponent<Rigidbody>();

        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        rb.linearVelocity = dir * strike_force;

        Vector3 spinAxis = Vector3.Cross(Vector3.up, dir);
        rb.angularVelocity = spinAxis * (strike_force * 2.8f);

        stick_animator.SetBool("isStroke", false);

        yield return new WaitForSeconds(cooldownAfterStrike);

        stick_prefab.SetActive(false);

        yield return new WaitUntil(() => gc.IsReadyToMove());

        isStroke = false;
        gc.NextMove();
    }
    
    public void IncrementBallsScored() => balls_scored++;
    public int GetBallsScored() => balls_scored;

    public void AdjustValue(float axis, ref float value, float min_value, float max_value,
        float changing_factor, Action<float> action)
    {
        value += axis * changing_factor * Time.deltaTime;
        value = Math.Clamp(value, min_value, max_value);
        action(Mathf.InverseLerp(min_value, max_value, value));
    }

    public void FollowBall()
    {
        if (whiteBall == null)
        {
            whiteBall = GameObject.FindGameObjectWithTag("WhiteBall");
            if (whiteBall == null)
            {
                Debug.LogError("White ball not found!");
                return;
            }
        }

        if (camera_prefab.Follow != whiteBall.transform)
        {
            camera_prefab.Follow = whiteBall.transform;
        }

        stick_prefab.transform.position = whiteBall.transform.position;
    }

    public Team GetTeam() => team;
}