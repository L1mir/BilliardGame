using Unity.Cinemachine;
using UnityEngine;

public class CameraSwitcher : MonoBehaviour
{
    public static CameraSwitcher Instance { get; private set; }
    
    CinemachineCamera followCamera;
    [SerializeField] private CinemachineCamera topDownCamera;
    
    private int followPriority = 10;
    private int topDownPriority = 5;
    
    private void Awake()
    {
        Instance = this;
    }
    
    private void Start()
    {
        var cameras = FindObjectsOfType<CinemachineCamera>();
        foreach(var cam in cameras)
        {
            if(cam.Follow != null && cam.Follow.CompareTag("WhiteBall"))
            {
                followCamera = cam;
                break;
            }
        }
    }
    
    public void SwitchToTopDown()
    {
        followCamera.Priority = topDownPriority;
        topDownCamera.Priority = followPriority;
    }
    
    public void SwitchToFollow()
    {
        topDownCamera.Priority = topDownPriority;
        followCamera.Priority = followPriority;
    }
}