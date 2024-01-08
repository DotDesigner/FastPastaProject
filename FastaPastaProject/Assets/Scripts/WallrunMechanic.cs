using StarterAssets;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallrunMechanic : MonoBehaviour
{
    private FirstPersonController firstPersonController;

    [Header("Wall Running")]
    public float WallRunSpeed = 6.0f;
    public float WallRunGravity = -5.0f;
    public LayerMask WallLayers;

    private RaycastHit wallHit;
    private bool isWallRunning = false;

    private void Awake()
    {
        firstPersonController = GetComponent<FirstPersonController>();
    }

    private void Update()
    {
        CheckForWallRun();
    }

    private void CheckForWallRun()
    {
        if (IsTouchingWall() && !firstPersonController.Grounded)
        {
            StartWallRun();
        }
        else
        {
            StopWallRun();
        }
    }

    private bool IsTouchingWall()
    {
        if (Physics.Raycast(transform.position, transform.right, out wallHit, 1f, WallLayers) ||
            Physics.Raycast(transform.position, -transform.right, out wallHit, 1f, WallLayers))
        {
            return true;
        }
        return false;
    }

    private void StartWallRun()
    {
        isWallRunning = true;
        //firstPersonController._verticalVelocity = WallRunGravity;

    }

    private void StopWallRun()
    {
        if (isWallRunning)
        {
            isWallRunning = false;
        }
    }

    public void ApplyWallRunForces(float speed, float gravity)
    {

    }
}
