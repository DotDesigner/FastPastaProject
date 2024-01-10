using StarterAssets;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallrunMechanic : MonoBehaviour
{
    private FirstPersonController firstPersonController;
    private StarterAssetsInputs _input;

    [Header("Wall Running")]
    public float WallRunSpeed = 6.0f;
    public float WallRunGravity = -5.0f;
    public LayerMask WallLayers;

    private RaycastHit wallHit;
    private bool isWallRunning = false;
    private bool disableADKeys = false;
    private Vector3 wallRunDirection = Vector3.zero;

    private void Awake()
    {
        firstPersonController = GetComponent<FirstPersonController>();
        _input = GetComponent<StarterAssetsInputs>();
    }

    private void Update()
    {
        if (disableADKeys)
        {
            _input.move.x = 0;
        }
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
        if (Physics.Raycast(transform.position, transform.right, out wallHit, 1f, WallLayers))
        {
            wallRunDirection = -transform.right; // Opposite direction to the right wall
            return true;
        }
        else if (Physics.Raycast(transform.position, -transform.right, out wallHit, 1f, WallLayers))
        {
            wallRunDirection = transform.right; // Opposite direction to the left wall
            return true;
        }
        return false;
    }

    private void StartWallRun()
    {
        isWallRunning = true;
        firstPersonController._verticalVelocity = WallRunGravity;
        disableADKeys = true;
        JumpOffWall();

    }

    private void StopWallRun()
    {
        if (isWallRunning)
        {

            isWallRunning = false;
            disableADKeys = false;
        }
    }

    public void JumpOffWall()
    {

    }

}