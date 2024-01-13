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
    public float Offset = 100f;

    private RaycastHit wallHit;
    public bool isWallRunning = false;
    public bool disableADKeys = false;
    private Vector3 wallRunDirection = Vector3.zero;
    public bool isRightwardJump = false;
    public bool isLeftwardJump = false;

    public float wallRunCooldown = 1.0f; // Cooldown duration in seconds
    private float wallRunCooldownTimer = 0.0f;

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
            Debug.Log("Disabled");
        }
        CheckForWallRun();

        if (wallRunCooldownTimer > 0)
        {
            wallRunCooldownTimer -= Time.deltaTime;
        }
    }

    private void CheckForWallRun()
    {
        if (IsTouchingWall() && !firstPersonController.Grounded && wallRunCooldownTimer <= 0)
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
        if (Physics.Raycast(transform.position, transform.right, out wallHit, 1.5f, WallLayers))
        {
            wallRunDirection = -transform.right;
            isRightwardJump = true;
// Opposite direction to the right wall
            return true;

        }
        else if (Physics.Raycast(transform.position, -transform.right, out wallHit, 1.5f, WallLayers))
        {
            wallRunDirection = transform.right; // Opposite direction to the left wall
            isLeftwardJump = true;
            return true;
        }
        return false;
    }

    private void StartWallRun()
    {
        isWallRunning = true;
        firstPersonController._verticalVelocity = WallRunGravity;
        disableADKeys = true;

    }

    private void StopWallRun()
    {
        if (isWallRunning)
        {
            isWallRunning = false;
            disableADKeys = false;
        }
        isRightwardJump = false;
        isLeftwardJump = false;
    }

    public void JumpOffWall()
    {
        wallRunCooldownTimer = wallRunCooldown;
    }

}