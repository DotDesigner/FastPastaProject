using UnityEngine;

public class SlideMechanic : MonoBehaviour
{

    private FirstPersonPlayerController playerController;
    public float maxSlideSpeed = 10f;
    public float slideSmoothTime = 0.5f; // Time taken to smooth the slide transition

    private CharacterController characterController;
    private bool isSliding = false;
    private Vector3 slideVelocity; // Current velocity while sliding
    public float slideBoostFromJumpMultiply = 1.5f;
    private float slopeAngle;

    void Start()
    {
        playerController = gameObject.GetComponent<FirstPersonPlayerController>();
        characterController = gameObject.GetComponent<CharacterController>();
    }

    void Update()
    {
        Vector3 hitNormal;
        float slopeAngle;
        if (playerController.isGrounded && Input.GetButton("Slide") && IsOnSlope(out hitNormal, out slopeAngle))
        {
            isSliding = true;
            HandleSlide(hitNormal);
        }
        else
        {
            // Check if the player was sliding and has now released the slide button
            if (isSliding)
            {
                isSliding = false;
                // Optionally, you could apply a final lerp to smooth the transition
                playerController.currentVelocity = Vector3.Lerp(slideVelocity, playerController.currentVelocity, slideSmoothTime * Time.deltaTime);
            }
        }

        // Apply sliding velocity only if sliding
        if (isSliding)
        {
            playerController.currentVelocity = slideVelocity;
        }
    }

    private void HandleSlide(Vector3 hitNormal)
    {
        Vector3 slideDirection = new Vector3(hitNormal.x, -hitNormal.y, hitNormal.z).normalized;
        float targetSlideSpeed = Mathf.Lerp(playerController.currentVelocity.magnitude, maxSlideSpeed, slopeAngle / 90f);

        if (slopeAngle <= 2)
        {
            targetSlideSpeed *= slideBoostFromJumpMultiply;
        }

        slideVelocity = Vector3.Lerp(slideVelocity, slideDirection * targetSlideSpeed, slideSmoothTime * Time.deltaTime);
    }

    private bool IsOnSlope(out Vector3 hitNormal, out float slopeAngle)
    {
        RaycastHit hit;
        float checkDistance = characterController.height / 2 + 0.1f;
        bool hitDetected = Physics.Raycast(transform.position, Vector3.down, out hit, checkDistance);
        hitNormal = Vector3.up;
        slopeAngle = 0f;

        if (hitDetected)
        {
            hitNormal = hit.normal;
            slopeAngle = Vector3.Angle(Vector3.up, hitNormal);
            return true;
        }

        return false;
    }
}