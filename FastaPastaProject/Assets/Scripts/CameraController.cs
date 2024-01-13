using Cinemachine;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public static CameraController instanceCamera;
    public CinemachineVirtualCamera vc;
    private Cinemachine3rdPersonFollow follow;

    private void Awake()
    {
        instanceCamera = this;
    }

    private void Start()
    {

        follow = vc.GetCinemachineComponent<Cinemachine3rdPersonFollow>();
        follow.ShoulderOffset = new Vector3(0, 0, 0);
    }

    public void StartSlideCamera()
    {
        follow.ShoulderOffset = new Vector3(0,-0.5f,0);
    }
    public void StopSlideCamera()
    {
        follow.ShoulderOffset = new Vector3(0, 0, 0);
    }
}
