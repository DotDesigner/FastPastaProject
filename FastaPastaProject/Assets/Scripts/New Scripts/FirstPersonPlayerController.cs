using Cinemachine;
using UnityEngine;
using TMPro;

public class FirstPersonPlayerController : MonoBehaviour
{
    [Header("CameraSettings")]
    public float mouseSensitivity = 100f;
    public GameObject playerTarget;

    [Header("MovementSettings")]
    public float moveSpeed = 5f; // Speed at which the player moves
    private float smoothTime = 0.5f; // Time taken to smooth the player's movement
    public float acceleration;
    public float decceleration;

    [Header("JumpSettings")]
    public float jumpForce = 5f; // Force of the jump
    public float gravity = -9.81f;
    public float fallMultiplier = 2.5f;
    public float jumpDelay = 0.1f; // Delay before next jump is allowed

    [Header("NotChangableValues")]
    public float speed;
    public TextMeshProUGUI speedText; // Add this field


    private float xRotation = 0f;
    private bool isPauseGame;
    private CinemachineVirtualCamera cinemachineCamera;
    private CharacterController characterController; // CharacterController component
    private Vector3 currentVelocity; // Store the velocity of the player
    private bool isGrounded;
    private float jumpTimeoutDelta;
    private bool isMoving = false;



    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        cinemachineCamera = GetComponentInChildren<CinemachineVirtualCamera>();
        characterController = GetComponent<CharacterController>();
        jumpTimeoutDelta = 0;
    }

    void Update()
    {
        CameraRotation();
        PauseGame();
        PlayerMovement();

    }

    private void CameraRotation()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f); // Prevents over-rotation

        // Apply vertical rotation to the camera
        if (cinemachineCamera != null)
        {
            cinemachineCamera.transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        }

        // Apply horizontal rotation to the player
        transform.Rotate(Vector3.up * mouseX);
    }
    private void PauseGame()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && !isPauseGame)
        {
            Time.timeScale = 0;
            Cursor.lockState = CursorLockMode.None;
            isPauseGame = true;
        }
        else if (Input.GetKeyDown(KeyCode.Escape) && isPauseGame)
        {
            Time.timeScale = 1;
            Cursor.lockState = CursorLockMode.Locked;
            isPauseGame = false;
        }
    }

    private void PlayerMovement()
    {
        speed = new Vector3(currentVelocity.x, 0, currentVelocity.z).magnitude;
        if (speedText != null)
        {
            speedText.text = "Speed: " + speed.ToString("F2"); // F2 formats to 2 decimal places
        }
        if (speed < 0.01f)
        {
            speed = 0;
            currentVelocity.x = 0;
            currentVelocity.z = 0;
        }

        isGrounded = characterController.isGrounded;
        if (isGrounded && currentVelocity.y < 0)
        {
            currentVelocity.y = -2f; // Small downward force to keep the player grounded
        }

        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        Vector3 targetDirection = transform.right * horizontal + transform.forward * vertical;
        if (targetDirection.magnitude > 1)
        {
            targetDirection.Normalize();
        }

        // Calculate the desired velocity based on input
        Vector3 desiredVelocity = targetDirection * moveSpeed;
        desiredVelocity.y = currentVelocity.y; // Keep the vertical component for gravity and jumping

        // Check if the player has started or stopped moving
        if (horizontal != 0 || vertical != 0)
        {
            if (!isMoving)
            {
                // Player starts moving
                smoothTime = acceleration; // Faster acceleration
                isMoving = true;
            }
        }
        else if (isMoving)
        {
            // Player stops moving
            smoothTime = decceleration; // Slower deceleration
            isMoving = false;
        }

        // Apply the current deceleration value
        currentVelocity = Vector3.Lerp(currentVelocity, horizontal == 0 && vertical == 0 ? new Vector3(0, currentVelocity.y, 0) : desiredVelocity, smoothTime * Time.deltaTime);

        // Jump logic
        if (Input.GetButton("Jump") && isGrounded && jumpTimeoutDelta <= 0)
        {
            currentVelocity.y = Mathf.Sqrt(jumpForce * -2f * gravity);
            jumpTimeoutDelta = jumpDelay;
        }

        // Gravity logic
        if (!isGrounded)
        {
            currentVelocity.y += gravity * fallMultiplier * Time.deltaTime;
        }
        else if (currentVelocity.y < 0)
        {
            currentVelocity.y += gravity * Time.deltaTime;
        }

        // Apply reduced velocity when jump button is released in the air
        if (Input.GetButtonUp("Jump") && currentVelocity.y > 0)
        {
            currentVelocity.y *= 0.5f;
        }

        // Move the character controller
        characterController.Move(currentVelocity * Time.deltaTime);

        // Countdown jump timeout
        if (jumpTimeoutDelta >= 0.0f)
        {
            jumpTimeoutDelta -= Time.deltaTime;
        }
    }
}